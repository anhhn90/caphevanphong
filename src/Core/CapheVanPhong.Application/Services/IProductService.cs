#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Application.Services;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByBrandAsync(int brandId, CancellationToken cancellationToken = default);

    Task<(bool success, string? error)> CreateAsync(
        string name,
        string slug,
        int brandId,
        IReadOnlyList<int> categoryIds,
        bool isAvailable,
        decimal? price,
        string? origin,
        string? model,
        int? numberOfGroupHeads,
        string? voltage,
        string? power,
        string? dimensions,
        string? weight,
        string? condition,
        string? warrantyPeriod,
        string? salesRegion,
        string? description,
        string? capacity,
        string? material,
        IReadOnlyList<NewImageUpload> images,
        CancellationToken cancellationToken = default);

    Task<(bool success, string? error)> UpdateAsync(
        int id,
        string name,
        string slug,
        int brandId,
        IReadOnlyList<int> categoryIds,
        bool isAvailable,
        decimal? price,
        string? origin,
        string? model,
        int? numberOfGroupHeads,
        string? voltage,
        string? power,
        string? dimensions,
        string? weight,
        string? condition,
        string? warrantyPeriod,
        string? salesRegion,
        string? description,
        string? capacity,
        string? material,
        IReadOnlyList<KeptImageInput> keptImages,
        IReadOnlyList<NewImageUpload> newImages,
        CancellationToken cancellationToken = default);

    Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

/// <summary>A new file to be saved as a product image.</summary>
public sealed record NewImageUpload(Stream Content, string OriginalFileName, bool IsMain, int DisplayOrder);

/// <summary>An already-stored product image to be retained during an update.</summary>
public sealed record KeptImageInput(string ImageName, bool IsMain, int DisplayOrder);
