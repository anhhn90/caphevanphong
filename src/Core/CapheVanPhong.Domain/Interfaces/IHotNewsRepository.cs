#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Domain.Interfaces;

public interface IHotNewsRepository : IRepository<HotNews>
{
    Task<IReadOnlyList<HotNews>> GetActiveAsync(CancellationToken cancellationToken = default);
}
