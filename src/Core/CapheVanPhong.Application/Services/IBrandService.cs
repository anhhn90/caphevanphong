#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Application.Services;

public interface IBrandService
{
    Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Brand?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<(bool success, string? error)> CreateAsync(string name, string slug, string? description, string? logoName, int displayOrder, CancellationToken cancellationToken = default);
    Task<(bool success, string? error)> UpdateAsync(int id, string name, string slug, string? description, string? logoName, int displayOrder, bool isActive, CancellationToken cancellationToken = default);
    Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
