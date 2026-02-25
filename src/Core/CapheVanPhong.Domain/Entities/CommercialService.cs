#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class CommercialService : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Introduction { get; private set; } = string.Empty;
    public string? Content { get; private set; }
    public string IconClass { get; private set; } = string.Empty;
    public string? ImageName { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    private CommercialService() { } // EF Core constructor

    public static CommercialService Create(
        string title,
        string slug,
        string introduction,
        string? content,
        string iconClass,
        string? imageName = null,
        bool isActive = true,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty.", nameof(slug));
        if (string.IsNullOrWhiteSpace(introduction))
            throw new ArgumentException("Introduction cannot be empty.", nameof(introduction));
        if (string.IsNullOrWhiteSpace(iconClass))
            throw new ArgumentException("IconClass cannot be empty.", nameof(iconClass));

        return new CommercialService
        {
            Title = title.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            Introduction = introduction,
            Content = string.IsNullOrWhiteSpace(content) ? null : content,
            IconClass = iconClass.Trim(),
            ImageName = imageName,
            IsActive = isActive,
            DisplayOrder = displayOrder
        };
    }

    public void Update(
        string title,
        string slug,
        string introduction,
        string? content,
        string iconClass,
        string? imageName,
        bool isActive,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty.", nameof(slug));
        if (string.IsNullOrWhiteSpace(introduction))
            throw new ArgumentException("Introduction cannot be empty.", nameof(introduction));
        if (string.IsNullOrWhiteSpace(iconClass))
            throw new ArgumentException("IconClass cannot be empty.", nameof(iconClass));

        Title = title.Trim();
        Slug = slug.ToLowerInvariant().Trim();
        Introduction = introduction;
        Content = string.IsNullOrWhiteSpace(content) ? null : content;
        IconClass = iconClass.Trim();
        ImageName = imageName;
        IsActive = isActive;
        DisplayOrder = displayOrder;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
