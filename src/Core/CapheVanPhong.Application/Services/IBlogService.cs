#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Application.Services;

public interface IBlogService
{
    Task<IReadOnlyList<Blog>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Blog>> GetByCategoryAsync(int categoryId, bool activeOnly = true, CancellationToken ct = default);
    Task<Blog?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Blog?> GetBySlugAsync(string blogSlug, CancellationToken ct = default);
    Task<(bool Success, string? Error)> CreateAsync(int categoryId, string title, string introduction, string fullContent, bool isActive, Stream? imageStream, string? imageFileName, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateAsync(int id, int categoryId, string title, string introduction, string fullContent, bool isActive, string? existingImageName, Stream? newImageStream, string? newImageFileName, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct = default);
}
