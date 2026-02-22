#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class Blog : BaseEntity
{
    public int BlogCategoryId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? ImageName { get; private set; }
    public string Introduction { get; private set; } = string.Empty;
    public string FullContent { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public BlogCategory Category { get; private set; } = null!;

    private Blog() { } // EF Core constructor

    public static Blog Create(
        int blogCategoryId,
        string title,
        string slug,
        string introduction,
        string fullContent,
        string? imageName = null,
        bool isActive = true)
    {
        if (blogCategoryId <= 0)
            throw new ArgumentException("BlogCategoryId must be positive.", nameof(blogCategoryId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty.", nameof(slug));
        if (string.IsNullOrWhiteSpace(introduction))
            throw new ArgumentException("Introduction cannot be empty.", nameof(introduction));
        if (string.IsNullOrWhiteSpace(fullContent))
            throw new ArgumentException("Full content cannot be empty.", nameof(fullContent));

        return new Blog
        {
            BlogCategoryId = blogCategoryId,
            Title = title.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            ImageName = imageName,
            Introduction = introduction,
            FullContent = fullContent,
            IsActive = isActive
        };
    }

    public void Update(
        int blogCategoryId,
        string title,
        string slug,
        string introduction,
        string fullContent,
        string? imageName,
        bool isActive)
    {
        if (blogCategoryId <= 0)
            throw new ArgumentException("BlogCategoryId must be positive.", nameof(blogCategoryId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty.", nameof(slug));
        if (string.IsNullOrWhiteSpace(introduction))
            throw new ArgumentException("Introduction cannot be empty.", nameof(introduction));
        if (string.IsNullOrWhiteSpace(fullContent))
            throw new ArgumentException("Full content cannot be empty.", nameof(fullContent));

        BlogCategoryId = blogCategoryId;
        Title = title.Trim();
        Slug = slug.ToLowerInvariant().Trim();
        ImageName = imageName;
        Introduction = introduction;
        FullContent = fullContent;
        IsActive = isActive;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
