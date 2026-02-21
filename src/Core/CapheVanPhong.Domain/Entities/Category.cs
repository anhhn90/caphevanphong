#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? ImageName { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Self-referencing hierarchy
    public int? ParentId { get; private set; }
    public int Level { get; private set; } // 0=root, 1=child, 2=grandchild (max)

    public Category? Parent { get; private set; }
    public ICollection<Category> Children { get; private set; } = new List<Category>();

    // M:N with Products via ProductCategory
    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();

    private Category() { } // EF Core constructor

    public static Category Create(
        string name,
        string slug,
        string description,
        int? parentId,
        int? parentLevel,
        string? imageName = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên danh mục không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug không được để trống", nameof(slug));

        int newLevel = parentId.HasValue ? (parentLevel ?? 0) + 1 : 0;
        if (newLevel > 2)
            throw new InvalidOperationException("Không thể tạo danh mục quá 2 cấp (gốc → con → cháu)");

        return new Category
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            Description = description,
            ImageName = imageName,
            DisplayOrder = displayOrder,
            IsActive = true,
            ParentId = parentId,
            Level = newLevel,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string slug,
        string description,
        int? parentId,
        int? parentLevel,
        string? imageName = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên danh mục không được để trống", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug không được để trống", nameof(slug));

        int newLevel = parentId.HasValue ? (parentLevel ?? 0) + 1 : 0;
        if (newLevel > 2)
            throw new InvalidOperationException("Không thể tạo danh mục quá 2 cấp (gốc → con → cháu)");

        Name = name;
        Slug = slug.ToLowerInvariant();
        Description = description;
        ImageName = imageName;
        DisplayOrder = displayOrder;
        ParentId = parentId;
        Level = newLevel;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}
