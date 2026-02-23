#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CapheVanPhong.Infrastructure.Persistence.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Product>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
            .OrderByDescending(p => p.Id)
            .ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
        => _dbSet.AnyAsync(
            p => p.Slug == slug && (excludeId == null || p.Id != excludeId),
            cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByCategoryIdsAsync(
        IEnumerable<int> categoryIds,
        CancellationToken cancellationToken = default)
    {
        var ids = categoryIds.ToList();
        if (ids.Count == 0)
            return [];

        return await _dbSet
            .Where(p => p.ProductCategories.Any(pc => ids.Contains(pc.CategoryId)))
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
            .Distinct()
            .OrderByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }
}
