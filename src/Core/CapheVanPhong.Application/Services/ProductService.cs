#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;

namespace CapheVanPhong.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public Task<IReadOnlyList<Product>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        => _productRepository.GetAllWithDetailsAsync(cancellationToken);

    public Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        => _productRepository.GetByIdWithDetailsAsync(id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        // Collect self + all descendant IDs, then query
        var descendantIds = await _categoryRepository.GetAllDescendantIdsAsync(categoryId, cancellationToken);
        var allIds = new List<int>(descendantIds.Count + 1) { categoryId };
        allIds.AddRange(descendantIds);
        return await _productRepository.GetByCategoryIdsAsync(allIds, cancellationToken);
    }

    public Task<IReadOnlyList<Product>> GetByBrandAsync(int brandId, CancellationToken cancellationToken = default)
        => _productRepository.GetByCategoryIdsAsync([], cancellationToken); // placeholder; extend if needed

    public async Task<(bool success, string? error)> CreateAsync(
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
        CancellationToken cancellationToken = default)
    {
        if (await _productRepository.SlugExistsAsync(slug, null, cancellationToken))
            return (false, $"Slug '{slug}' is already in use.");

        var validationError = await ValidateReferencesAsync(brandId, categoryIds, cancellationToken);
        if (validationError is not null)
            return (false, validationError);

        var product = Product.Create(
            name: name,
            slug: slug,
            description: description,
            price: price,
            brandId: brandId,
            categoryIds: categoryIds,
            origin: origin,
            model: model,
            numberOfGroupHeads: numberOfGroupHeads,
            voltage: voltage,
            power: power,
            dimensions: dimensions,
            weight: weight,
            condition: condition,
            warrantyPeriod: warrantyPeriod,
            salesRegion: salesRegion,
            capacity: capacity,
            material: material);

        product.SetAvailability(isAvailable);

        var savedImages = await SaveNewImagesAsync(images, cancellationToken);
        product.ReplaceImages(MapImages(0, savedImages));

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task<(bool success, string? error)> UpdateAsync(
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
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (product is null)
            return (false, "Product not found.");

        if (await _productRepository.SlugExistsAsync(slug, id, cancellationToken))
            return (false, $"Slug '{slug}' is already in use.");

        var validationError = await ValidateReferencesAsync(brandId, categoryIds, cancellationToken);
        if (validationError is not null)
            return (false, validationError);

        product.Update(
            name: name,
            slug: slug,
            description: description,
            price: price,
            brandId: brandId,
            origin: origin,
            model: model,
            numberOfGroupHeads: numberOfGroupHeads,
            voltage: voltage,
            power: power,
            dimensions: dimensions,
            weight: weight,
            condition: condition,
            warrantyPeriod: warrantyPeriod,
            salesRegion: salesRegion,
            capacity: capacity,
            material: material);

        product.SetAvailability(isAvailable);
        product.SetCategories(categoryIds);

        var oldImageNames = product.ProductImages.Select(i => i.ImageName).ToList();

        var savedNewImages = await SaveNewImagesAsync(newImages, cancellationToken);

        var allImages = keptImages
            .Select(i => new ImageEntry(i.ImageName, i.IsMain, i.DisplayOrder))
            .Concat(savedNewImages.Select(i => new ImageEntry(i.ImageName, i.IsMain, i.DisplayOrder)))
            .ToList();

        var keptImageNames = allImages.Select(i => i.ImageName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        product.ReplaceImages(MapImages(product.Id, allImages));
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var oldImage in oldImageNames.Where(x => !keptImageNames.Contains(x)))
            await _fileStorageService.RenameToDeletedAsync("products", oldImage, cancellationToken);

        return (true, null);
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (product is null)
            return (false, "Product not found.");

        var imageNames = product.ProductImages.Select(i => i.ImageName).ToList();

        _productRepository.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var imageName in imageNames)
            await _fileStorageService.RenameToDeletedAsync("products", imageName, cancellationToken);

        return (true, null);
    }

    private async Task<string?> ValidateReferencesAsync(
        int brandId,
        IReadOnlyList<int> categoryIds,
        CancellationToken cancellationToken)
    {
        if (categoryIds.Count == 0)
            return "At least one category must be selected.";

        if (await _brandRepository.GetByIdAsync(brandId, cancellationToken) is null)
            return "Brand not found.";

        foreach (var categoryId in categoryIds)
        {
            if (await _categoryRepository.GetByIdAsync(categoryId, cancellationToken) is null)
                return $"Category with ID {categoryId} not found.";
        }

        return null;
    }

    private async Task<List<ImageEntry>> SaveNewImagesAsync(
        IReadOnlyList<NewImageUpload> uploads,
        CancellationToken cancellationToken)
    {
        var result = new List<ImageEntry>(uploads.Count);
        foreach (var upload in uploads)
        {
            var fileName = $"{Guid.NewGuid():N}_{upload.OriginalFileName}";
            var savedName = await _fileStorageService.SaveAsync("products", fileName, upload.Content, cancellationToken);
            result.Add(new ImageEntry(savedName, upload.IsMain, upload.DisplayOrder));
        }
        return result;
    }

    private static IReadOnlyList<ProductImage> MapImages(int productId, IReadOnlyList<ImageEntry> images)
    {
        if (images.Count == 0)
            return Array.Empty<ProductImage>();

        var normalizedImages = images
            .Where(i => !string.IsNullOrWhiteSpace(i.ImageName))
            .Select((i, index) => new ImageEntry(i.ImageName.Trim(), i.IsMain, i.DisplayOrder == 0 ? index : i.DisplayOrder))
            .ToList();

        if (normalizedImages.Count == 0)
            return Array.Empty<ProductImage>();

        if (normalizedImages.Count(i => i.IsMain) != 1)
        {
            for (var i = 0; i < normalizedImages.Count; i++)
                normalizedImages[i] = normalizedImages[i] with { IsMain = i == 0 };
        }

        return normalizedImages
            .Select(i => ProductImage.Create(
                productId: productId,
                imageName: i.ImageName,
                isMain: i.IsMain,
                displayOrder: i.DisplayOrder))
            .ToList();
    }

    // Internal DTO used only within ProductService to shuttle image data between helpers.
    private sealed record ImageEntry(string ImageName, bool IsMain, int DisplayOrder);
}
