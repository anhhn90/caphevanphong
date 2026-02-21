#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllWithChildrenAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdWithChildrenAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetAllDescendantIdsAsync(int parentCategoryId, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
}
