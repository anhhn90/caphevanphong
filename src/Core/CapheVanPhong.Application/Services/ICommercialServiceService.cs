#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Application.Services;

public interface ICommercialServiceService
{
    Task<IReadOnlyList<CommercialService>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CommercialService>> GetActiveAsync(CancellationToken ct = default);
    Task<CommercialService?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CommercialService?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<(bool Success, string? Error)> CreateAsync(string title, string introduction, string? content, string iconClass, bool isActive, int displayOrder, Stream? imageStream, string? imageFileName, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateAsync(int id, string title, string introduction, string? content, string iconClass, bool isActive, int displayOrder, string? existingImageName, Stream? newImageStream, string? newImageFileName, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct = default);
}
