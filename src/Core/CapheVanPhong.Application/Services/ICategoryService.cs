#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Application.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllWithChildrenAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdWithChildrenAsync(int id, CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Returns the given category's ID plus all descendant IDs (for product filtering).</summary>
    Task<IReadOnlyList<int>> GetSelfAndDescendantIdsAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<(bool success, string? error)> CreateAsync(
        string name, string slug, string description,
        int? parentId, string? imageName, int displayOrder,
        CancellationToken cancellationToken = default);

    Task<(bool success, string? error)> UpdateAsync(
        int id, string name, string slug, string description,
        int? parentId, string? imageName, int displayOrder, bool isActive,
        CancellationToken cancellationToken = default);

    Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
