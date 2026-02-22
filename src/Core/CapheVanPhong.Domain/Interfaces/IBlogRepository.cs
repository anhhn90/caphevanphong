#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Domain.Interfaces;

public interface IBlogRepository : IRepository<Blog>
{
    Task<IReadOnlyList<Blog>> GetByCategoryIdAsync(int categoryId, bool activeOnly = true, CancellationToken ct = default);
    Task<Blog?> GetBySlugAsync(string blogSlug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
}
