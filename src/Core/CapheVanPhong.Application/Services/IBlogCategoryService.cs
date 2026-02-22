#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Application.Services;

public interface IBlogCategoryService
{
    Task<IReadOnlyList<BlogCategory>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BlogCategory>> GetActiveAsync(CancellationToken ct = default);
    Task<BlogCategory?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(bool Success, string? Error)> CreateAsync(string name, string? description, bool isActive, int displayOrder, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateAsync(int id, string name, string? description, bool isActive, int displayOrder, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct = default);
}
