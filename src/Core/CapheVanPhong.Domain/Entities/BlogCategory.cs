#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class BlogCategory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    private readonly List<Blog> _blogs = new();
    public IReadOnlyList<Blog> Blogs => _blogs.AsReadOnly();

    private BlogCategory() { } // EF Core constructor

    public static BlogCategory Create(
        string name,
        string slug,
        string? description = null,
        bool isActive = true,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty.", nameof(slug));

        return new BlogCategory
        {
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = isActive,
            DisplayOrder = displayOrder
        };
    }

    public void Update(string name, string slug, string? description, bool isActive, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty.", nameof(slug));

        Name = name.Trim();
        Slug = slug.ToLowerInvariant().Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsActive = isActive;
        DisplayOrder = displayOrder;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
