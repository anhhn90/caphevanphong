#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LogoName { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Brand() { } // EF Core constructor

    public static Brand Create(string name, string slug, string? description = null, string? logoName = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên thương hiệu không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug không được để trống", nameof(slug));

        return new Brand
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            Description = description,
            LogoName = logoName,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string slug, string? description = null, string? logoName = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên thương hiệu không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug không được để trống", nameof(slug));

        Name = name;
        Slug = slug.ToLowerInvariant();
        Description = description;
        LogoName = logoName;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}
