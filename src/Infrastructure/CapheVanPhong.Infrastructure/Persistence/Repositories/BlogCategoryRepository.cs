#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CapheVanPhong.Infrastructure.Persistence.Repositories;

public class BlogCategoryRepository : Repository<BlogCategory>, IBlogCategoryRepository
{
    public BlogCategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<BlogCategory>> GetActiveOrderedAsync(CancellationToken ct = default)
        => await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

    public Task<BlogCategory?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default)
        => _dbSet.AnyAsync(c => c.Slug == slug && (excludeId == null || c.Id != excludeId), ct);
}
