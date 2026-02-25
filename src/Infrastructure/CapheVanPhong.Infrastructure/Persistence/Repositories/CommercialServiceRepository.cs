#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CapheVanPhong.Infrastructure.Persistence.Repositories;

public class CommercialServiceRepository : Repository<CommercialService>, ICommercialServiceRepository
{
    public CommercialServiceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<CommercialService>> GetActiveOrderedAsync(CancellationToken ct = default)
        => await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Title)
            .ToListAsync(ct);

    public Task<CommercialService?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(s => s.Slug == slug, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default)
        => _dbSet.AnyAsync(s => s.Slug == slug && (excludeId == null || s.Id != excludeId), ct);
}
