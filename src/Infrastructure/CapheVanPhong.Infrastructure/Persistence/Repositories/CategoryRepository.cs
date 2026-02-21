#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CapheVanPhong.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(c => c.Children.OrderBy(ch => ch.DisplayOrder))
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetAllWithChildrenAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(c => c.Children.Where(ch => ch.IsActive).OrderBy(ch => ch.DisplayOrder))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<Category?> GetByIdWithChildrenAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetAllDescendantIdsAsync(int parentCategoryId, CancellationToken cancellationToken = default)
    {
        // In-memory BFS/DFS traversal (practical for shallow hierarchies with max 2 levels)
        var allCategories = await _dbSet
            .Select(c => new { c.Id, c.ParentId })
            .ToListAsync(cancellationToken);

        var result = new List<int>();
        var queue = new Queue<int>();

        // Seed queue with direct children of parentCategoryId
        foreach (var c in allCategories.Where(c => c.ParentId == parentCategoryId))
            queue.Enqueue(c.Id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);
            foreach (var child in allCategories.Where(c => c.ParentId == current))
                queue.Enqueue(child.Id);
        }

        return result;
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(c => c.Slug == slug && (excludeId == null || c.Id != excludeId), cancellationToken);
}
