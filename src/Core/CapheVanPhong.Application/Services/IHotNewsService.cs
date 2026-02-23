#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Application.Services;

public interface IHotNewsService
{
    Task<IReadOnlyList<HotNews>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<HotNews?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HotNews>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<(bool success, string? error)> CreateAsync(
        string title,
        string content,
        bool isActive,
        Stream? imageStream = null,
        string? originalFileName = null,
        CancellationToken cancellationToken = default);

    Task<(bool success, string? error)> UpdateAsync(
        int id,
        string title,
        string content,
        bool isActive,
        string? existingImageName = null,
        Stream? newImageStream = null,
        string? newOriginalFileName = null,
        CancellationToken cancellationToken = default);

    Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<(bool success, string? error)> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
}
