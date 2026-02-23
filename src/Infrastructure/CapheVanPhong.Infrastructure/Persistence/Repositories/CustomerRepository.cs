#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CapheVanPhong.Infrastructure.Persistence.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Customer>> GetAllWithRepresentativesAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(c => c.Representatives)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<Customer?> GetByIdWithRepresentativesAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(c => c.Representatives)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Customer>> GetGoldCustomersAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(c => c.IsGoldCustomer && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CustomerRepresentative>> GetHomepageTestimonialsAsync(CancellationToken cancellationToken = default)
        => await _context.Set<CustomerRepresentative>()
            .Include(r => r.Customer)
            .Where(r => r.IsShowOnHomepage && r.Customer.IsActive)
            .OrderByDescending(r => r.StarRating)
            .ToListAsync(cancellationToken);

    public async Task<CustomerRepresentative?> GetRepresentativeByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Set<CustomerRepresentative>()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task AddRepresentativeAsync(CustomerRepresentative representative, CancellationToken cancellationToken = default)
        => await _context.Set<CustomerRepresentative>().AddAsync(representative, cancellationToken);

    public void UpdateRepresentative(CustomerRepresentative representative)
        => _context.Set<CustomerRepresentative>().Update(representative);

    public void DeleteRepresentative(CustomerRepresentative representative)
        => _context.Set<CustomerRepresentative>().Remove(representative);

    public async Task<bool> RepresentativeExistsAsync(int customerId, string userId, int? excludeId = null, CancellationToken cancellationToken = default)
        => await _context.Set<CustomerRepresentative>()
            .AnyAsync(r => r.CustomerId == customerId
                        && r.UserId == userId
                        && (excludeId == null || r.Id != excludeId),
                cancellationToken);
}
