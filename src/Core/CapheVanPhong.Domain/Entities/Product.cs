#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal? Price { get; private set; }
    public string? Origin { get; private set; }
    public string? Model { get; private set; }
    public int? NumberOfGroupHeads { get; private set; }
    public string? Voltage { get; private set; }
    public string? Power { get; private set; }
    public string? Dimensions { get; private set; }
    public string? Weight { get; private set; }
    public string? Condition { get; private set; }
    public string? WarrantyPeriod { get; private set; }
    public string? SalesRegion { get; private set; }
    public string? Capacity { get; private set; }
    public string? Material { get; private set; }
    public bool IsAvailable { get; private set; }
    public int? BrandId { get; private set; }

    public Brand? Brand { get; private set; }
    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
    public ICollection<ProductImage> ProductImages { get; private set; } = new List<ProductImage>();

    // M:N with Categories via ProductCategory
    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();

    private Product() { } // EF Core constructor

    public static Product Create(
        string name,
        string slug,
        string? description,
        decimal? price,
        int brandId,
        IEnumerable<int> categoryIds,
        string? origin = null,
        string? model = null,
        int? numberOfGroupHeads = null,
        string? voltage = null,
        string? power = null,
        string? dimensions = null,
        string? weight = null,
        string? condition = null,
        string? warrantyPeriod = null,
        string? salesRegion = null,
        string? capacity = null,
        string? material = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Product slug cannot be empty.", nameof(slug));

        if (brandId <= 0)
            throw new ArgumentException("BrandId is invalid.", nameof(brandId));
        if (price.HasValue && price.Value <= 0)
            throw new ArgumentException("Price must be greater than 0.", nameof(price));
        if (numberOfGroupHeads.HasValue && numberOfGroupHeads.Value <= 0)
            throw new ArgumentException("Number of group heads must be greater than 0.", nameof(numberOfGroupHeads));

        var product = new Product
        {
            Name = name,
            Slug = slug,
            Description = description,
            Price = price,
            BrandId = brandId,
            Origin = origin,
            Model = model,
            NumberOfGroupHeads = numberOfGroupHeads,
            Voltage = voltage,
            Power = power,
            Dimensions = dimensions,
            Weight = weight,
            Condition = condition,
            WarrantyPeriod = warrantyPeriod,
            SalesRegion = salesRegion,
            Capacity = capacity,
            Material = material,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var categoryId in categoryIds)
            product.ProductCategories.Add(new ProductCategory { ProductId = 0, CategoryId = categoryId });

        return product;
    }

    public void Update(
        string name,
        string slug,
        string? description,
        decimal? price,
        int brandId,
        string? origin = null,
        string? model = null,
        int? numberOfGroupHeads = null,
        string? voltage = null,
        string? power = null,
        string? dimensions = null,
        string? weight = null,
        string? condition = null,
        string? warrantyPeriod = null,
        string? salesRegion = null,
        string? capacity = null,
        string? material = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Product slug cannot be empty.", nameof(slug));

        if (brandId <= 0)
            throw new ArgumentException("BrandId is invalid.", nameof(brandId));
        if (price.HasValue && price.Value <= 0)
            throw new ArgumentException("Price must be greater than 0.", nameof(price));
        if (numberOfGroupHeads.HasValue && numberOfGroupHeads.Value <= 0)
            throw new ArgumentException("Number of group heads must be greater than 0.", nameof(numberOfGroupHeads));

        Name = name;
        Slug = slug;
        Description = description;
        Price = price;
        BrandId = brandId;
        Origin = origin;
        Model = model;
        NumberOfGroupHeads = numberOfGroupHeads;
        Voltage = voltage;
        Power = power;
        Dimensions = dimensions;
        Weight = weight;
        Condition = condition;
        WarrantyPeriod = warrantyPeriod;
        SalesRegion = salesRegion;
        Capacity = capacity;
        Material = material;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCategories(IEnumerable<int> categoryIds)
    {
        ProductCategories.Clear();
        foreach (var categoryId in categoryIds)
            ProductCategories.Add(new ProductCategory { ProductId = Id, CategoryId = categoryId });
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceImages(IEnumerable<ProductImage> images)
    {
        ProductImages.Clear();

        var materializedImages = images.ToList();
        if (materializedImages.Count > 0 && materializedImages.All(i => !i.IsMain))
            materializedImages[0].SetMain(true);

        foreach (var image in materializedImages)
            ProductImages.Add(image);

        UpdatedAt = DateTime.UtcNow;
    }
}
