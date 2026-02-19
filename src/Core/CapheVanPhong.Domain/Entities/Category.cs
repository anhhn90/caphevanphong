#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() { } // EF Core constructor

    public static Category Create(string name, string description, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên danh mục không được để trống", nameof(name));

        return new Category
        {
            Name = name,
            Description = description,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên danh mục không được để trống", nameof(name));

        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
