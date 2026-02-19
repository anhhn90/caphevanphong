#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsAvailable { get; private set; }
    public int CategoryId { get; private set; }
    
    public Category? Category { get; private set; }
    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    private Product() { } // EF Core constructor

    public static Product Create(string name, string description, decimal price, int categoryId, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên sản phẩm không được để trống", nameof(name));
        
        if (price <= 0)
            throw new ArgumentException("Giá sản phẩm phải lớn hơn 0", nameof(price));

        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId,
            ImageUrl = imageUrl,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, decimal price, int categoryId, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên sản phẩm không được để trống", nameof(name));
        
        if (price <= 0)
            throw new ArgumentException("Giá sản phẩm phải lớn hơn 0", nameof(price));

        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }
}
