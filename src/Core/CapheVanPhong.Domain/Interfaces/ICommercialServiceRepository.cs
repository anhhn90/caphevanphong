#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Domain.Interfaces;

public interface ICommercialServiceRepository : IRepository<CommercialService>
{
    Task<IReadOnlyList<CommercialService>> GetActiveOrderedAsync(CancellationToken ct = default);
    Task<CommercialService?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
}
