#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<IReadOnlyList<Customer>> GetAllWithRepresentativesAsync(CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdWithRepresentativesAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetGoldCustomersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerRepresentative>> GetHomepageTestimonialsAsync(CancellationToken cancellationToken = default);
    Task<CustomerRepresentative?> GetRepresentativeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddRepresentativeAsync(CustomerRepresentative representative, CancellationToken cancellationToken = default);
    void UpdateRepresentative(CustomerRepresentative representative);
    void DeleteRepresentative(CustomerRepresentative representative);
    Task<bool> RepresentativeExistsAsync(int customerId, string userId, int? excludeId = null, CancellationToken cancellationToken = default);
}
