#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CapheVanPhong.Infrastructure.Persistence.Repositories;

public class BrandRepository : Repository<Brand>, IBrandRepository
{
    public BrandRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(b => b.Slug == slug, cancellationToken);

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(b => b.Slug == slug && (excludeId == null || b.Id != excludeId), cancellationToken);
}
