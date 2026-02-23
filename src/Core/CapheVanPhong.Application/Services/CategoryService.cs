#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;

namespace CapheVanPhong.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => _categoryRepository.GetAllAsync(cancellationToken);

    public Task<IReadOnlyList<Category>> GetAllWithChildrenAsync(CancellationToken cancellationToken = default)
        => _categoryRepository.GetAllWithChildrenAsync(cancellationToken);

    public Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
        => _categoryRepository.GetRootCategoriesAsync(cancellationToken);

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _categoryRepository.GetByIdAsync(id, cancellationToken);

    public Task<Category?> GetByIdWithChildrenAsync(int id, CancellationToken cancellationToken = default)
        => _categoryRepository.GetByIdWithChildrenAsync(id, cancellationToken);

    public Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => _categoryRepository.GetBySlugAsync(slug, cancellationToken);

    public async Task<IReadOnlyList<int>> GetSelfAndDescendantIdsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var descendantIds = await _categoryRepository.GetAllDescendantIdsAsync(categoryId, cancellationToken);
        var result = new List<int>(descendantIds.Count + 1) { categoryId };
        result.AddRange(descendantIds);
        return result;
    }

    public async Task<(bool success, string? error)> CreateAsync(
        string name, string slug, string description,
        int? parentId, string? imageName, int displayOrder,
        CancellationToken cancellationToken = default)
    {
        if (await _categoryRepository.SlugExistsAsync(slug, null, cancellationToken))
            return (false, $"Slug '{slug}' đã được sử dụng.");

        int? parentLevel = null;
        if (parentId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(parentId.Value, cancellationToken);
            if (parent is null)
                return (false, "Danh mục cha không tồn tại.");
            if (parent.Level >= 2)
                return (false, "Không thể tạo danh mục quá 2 cấp.");
            parentLevel = parent.Level;
        }

        var category = Category.Create(name, slug, description, parentId, parentLevel, imageName, displayOrder);
        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool success, string? error)> UpdateAsync(
        int id, string name, string slug, string description,
        int? parentId, string? imageName, int displayOrder, bool isActive,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return (false, "Danh mục không tồn tại.");

        if (await _categoryRepository.SlugExistsAsync(slug, id, cancellationToken))
            return (false, $"Slug '{slug}' đã được sử dụng.");

        // Prevent circular reference
        if (parentId.HasValue)
        {
            if (parentId.Value == id)
                return (false, "Danh mục không thể là cha của chính nó.");

            var descendantIds = await _categoryRepository.GetAllDescendantIdsAsync(id, cancellationToken);
            if (descendantIds.Contains(parentId.Value))
                return (false, "Không thể tạo vòng lặp trong cấu trúc danh mục.");
        }

        int? parentLevel = null;
        if (parentId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(parentId.Value, cancellationToken);
            if (parent is null)
                return (false, "Danh mục cha không tồn tại.");
            if (parent.Level >= 2)
                return (false, "Không thể tạo danh mục quá 2 cấp.");
            parentLevel = parent.Level;
        }

        category.Update(name, slug, description, parentId, parentLevel, imageName, displayOrder);
        category.SetActive(isActive);
        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdWithChildrenAsync(id, cancellationToken);
        if (category is null)
            return (false, "Danh mục không tồn tại.");

        if (category.Children.Count > 0)
            return (false, "Không thể xóa danh mục còn danh mục con. Vui lòng xóa danh mục con trước.");

        var imageName = category.ImageName;

        _categoryRepository.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(imageName))
            await _fileStorageService.RenameToDeletedAsync("categories", imageName, cancellationToken);

        return (true, null);
    }
}
