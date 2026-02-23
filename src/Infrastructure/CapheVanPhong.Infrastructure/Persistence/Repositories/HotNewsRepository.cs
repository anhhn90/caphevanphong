#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CapheVanPhong.Infrastructure.Persistence.Repositories;

public class HotNewsRepository : Repository<HotNews>, IHotNewsRepository
{
    public HotNewsRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<HotNews>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(h => h.IsActive)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
}
