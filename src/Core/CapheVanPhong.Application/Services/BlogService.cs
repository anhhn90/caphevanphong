#nullable enable

using CapheVanPhong.Application.Helpers;
using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;

namespace CapheVanPhong.Application.Services;

public class BlogService : IBlogService
{
    private readonly IBlogRepository _blogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    private const string ImageSubfolder = "blogs";

    public BlogService(
        IBlogRepository blogRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _blogRepository = blogRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public Task<IReadOnlyList<Blog>> GetAllAsync(CancellationToken ct = default)
        => _blogRepository.GetAllAsync(ct);

    public Task<IReadOnlyList<Blog>> GetByCategoryAsync(int categoryId, bool activeOnly = true, CancellationToken ct = default)
        => _blogRepository.GetByCategoryIdAsync(categoryId, activeOnly, ct);

    public Task<Blog?> GetByIdAsync(int id, CancellationToken ct = default)
        => _blogRepository.GetByIdAsync(id, ct);

    public Task<Blog?> GetBySlugAsync(string blogSlug, CancellationToken ct = default)
        => _blogRepository.GetBySlugAsync(blogSlug, ct);

    public async Task<(bool Success, string? Error)> CreateAsync(
        int categoryId,
        string title,
        string introduction,
        string fullContent,
        bool isActive,
        Stream? imageStream,
        string? imageFileName,
        CancellationToken ct = default)
    {
        if (categoryId <= 0) return (false, "Please select a blog category.");
        if (string.IsNullOrWhiteSpace(title)) return (false, "Title is required.");
        if (string.IsNullOrWhiteSpace(introduction)) return (false, "Introduction is required.");
        if (string.IsNullOrWhiteSpace(fullContent)) return (false, "Full content is required.");

        var slug = SlugHelper.Generate(title);
        if (string.IsNullOrWhiteSpace(slug))
            return (false, "Could not generate a valid slug from the title.");

        if (await _blogRepository.SlugExistsAsync(slug, excludeId: null, ct))
            return (false, $"A blog with slug \"{slug}\" already exists.");

        string? storedImageName = null;
        if (imageStream is not null && !string.IsNullOrWhiteSpace(imageFileName))
        {
            var fileName = $"{Guid.NewGuid():N}_{imageFileName}";
            storedImageName = await _fileStorageService.SaveAsync(ImageSubfolder, fileName, imageStream, ct);
        }

        var blog = Blog.Create(categoryId, title, slug, introduction, fullContent, storedImageName, isActive);
        await _blogRepository.AddAsync(blog, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(
        int id,
        int categoryId,
        string title,
        string introduction,
        string fullContent,
        bool isActive,
        string? existingImageName,
        Stream? newImageStream,
        string? newImageFileName,
        CancellationToken ct = default)
    {
        if (categoryId <= 0) return (false, "Please select a blog category.");
        if (string.IsNullOrWhiteSpace(title)) return (false, "Title is required.");
        if (string.IsNullOrWhiteSpace(introduction)) return (false, "Introduction is required.");
        if (string.IsNullOrWhiteSpace(fullContent)) return (false, "Full content is required.");

        var blog = await _blogRepository.GetByIdAsync(id, ct);
        if (blog is null) return (false, "Blog not found.");

        var slug = SlugHelper.Generate(title);
        if (string.IsNullOrWhiteSpace(slug))
            return (false, "Could not generate a valid slug from the title.");

        if (await _blogRepository.SlugExistsAsync(slug, excludeId: id, ct))
            return (false, $"A blog with slug \"{slug}\" already exists.");

        var oldImageName = blog.ImageName;
        var imageName = existingImageName;

        if (newImageStream is not null && !string.IsNullOrWhiteSpace(newImageFileName))
        {
            var fileName = $"{Guid.NewGuid():N}_{newImageFileName}";
            imageName = await _fileStorageService.SaveAsync(ImageSubfolder, fileName, newImageStream, ct);
        }

        blog.Update(categoryId, title, slug, introduction, fullContent, imageName, isActive);
        _blogRepository.Update(blog);
        await _unitOfWork.SaveChangesAsync(ct);

        if (oldImageName is not null && !string.Equals(oldImageName, imageName, StringComparison.OrdinalIgnoreCase))
            await _fileStorageService.RenameToDeletedAsync(ImageSubfolder, oldImageName, ct);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        var blog = await _blogRepository.GetByIdAsync(id, ct);
        if (blog is null) return (false, "Blog not found.");

        var imageName = blog.ImageName;
        _blogRepository.Delete(blog);
        await _unitOfWork.SaveChangesAsync(ct);

        if (imageName is not null)
            await _fileStorageService.RenameToDeletedAsync(ImageSubfolder, imageName, ct);

        return (true, null);
    }
}
