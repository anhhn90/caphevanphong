#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class HotNews : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? ImageName { get; private set; }
    public bool IsActive { get; private set; } = true;

    private HotNews() { } // EF Core constructor

    public static HotNews Create(string title, string content, string? imageName = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        return new HotNews
        {
            Title = title.Trim(),
            Content = content,
            ImageName = imageName,
            IsActive = isActive
        };
    }

    public void Update(string title, string content, string? imageName, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        Title = title.Trim();
        Content = content;
        ImageName = imageName;
        IsActive = isActive;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
