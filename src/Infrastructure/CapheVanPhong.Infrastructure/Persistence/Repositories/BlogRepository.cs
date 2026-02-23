#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CapheVanPhong.Infrastructure.Persistence.Repositories;

public class BlogRepository : Repository<Blog>, IBlogRepository
{
    public BlogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Blog>> GetByCategoryIdAsync(int categoryId, bool activeOnly = true, CancellationToken ct = default)
        => await _dbSet
            .Include(b => b.Category)
            .Where(b => b.BlogCategoryId == categoryId && (!activeOnly || b.IsActive))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public Task<Blog?> GetBySlugAsync(string blogSlug, CancellationToken ct = default)
        => _dbSet
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Slug == blogSlug, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default)
        => _dbSet.AnyAsync(b => b.Slug == slug && (excludeId == null || b.Id != excludeId), ct);
}
