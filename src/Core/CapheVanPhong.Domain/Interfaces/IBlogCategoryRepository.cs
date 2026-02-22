#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Domain.Interfaces;

public interface IBlogCategoryRepository : IRepository<BlogCategory>
{
    Task<IReadOnlyList<BlogCategory>> GetActiveOrderedAsync(CancellationToken ct = default);
    Task<BlogCategory?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
}
