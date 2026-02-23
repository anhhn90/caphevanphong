#nullable enable

using CapheVanPhong.Application.Helpers;
using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;

namespace CapheVanPhong.Application.Services;

public class BlogCategoryService : IBlogCategoryService
{
    private readonly IBlogCategoryRepository _blogCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BlogCategoryService(IBlogCategoryRepository blogCategoryRepository, IUnitOfWork unitOfWork)
    {
        _blogCategoryRepository = blogCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<BlogCategory>> GetAllAsync(CancellationToken ct = default)
        => _blogCategoryRepository.GetAllAsync(ct);

    public Task<IReadOnlyList<BlogCategory>> GetActiveAsync(CancellationToken ct = default)
        => _blogCategoryRepository.GetActiveOrderedAsync(ct);

    public Task<BlogCategory?> GetByIdAsync(int id, CancellationToken ct = default)
        => _blogCategoryRepository.GetByIdAsync(id, ct);

    public async Task<(bool Success, string? Error)> CreateAsync(
        string name,
        string? description,
        bool isActive,
        int displayOrder,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Name is required.");

        var slug = SlugHelper.Generate(name);
        if (string.IsNullOrWhiteSpace(slug))
            return (false, "Could not generate a valid slug from the name.");

        if (await _blogCategoryRepository.SlugExistsAsync(slug, excludeId: null, ct))
            return (false, $"A blog category with slug \"{slug}\" already exists.");

        var category = BlogCategory.Create(name, slug, description, isActive, displayOrder);
        await _blogCategoryRepository.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(
        int id,
        string name,
        string? description,
        bool isActive,
        int displayOrder,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Name is required.");

        var category = await _blogCategoryRepository.GetByIdAsync(id, ct);
        if (category is null)
            return (false, "Blog category not found.");

        var slug = SlugHelper.Generate(name);
        if (string.IsNullOrWhiteSpace(slug))
            return (false, "Could not generate a valid slug from the name.");

        if (await _blogCategoryRepository.SlugExistsAsync(slug, excludeId: id, ct))
            return (false, $"A blog category with slug \"{slug}\" already exists.");

        category.Update(name, slug, description, isActive, displayOrder);
        _blogCategoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await _blogCategoryRepository.GetByIdAsync(id, ct);
        if (category is null)
            return (false, "Blog category not found.");

        _blogCategoryRepository.Delete(category);
        await _unitOfWork.SaveChangesAsync(ct);
        return (true, null);
    }
}
