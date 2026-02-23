#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class CustomerRepresentative : BaseEntity
{
    public int CustomerId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string? AvatarName { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string Position { get; private set; } = string.Empty;
    public string? Comment { get; private set; }
    public int StarRating { get; private set; } = 5;
    public bool IsShowOnHomepage { get; private set; }

    public Customer Customer { get; private set; } = null!;

    private CustomerRepresentative() { } // EF Core constructor

    public static CustomerRepresentative Create(
        int customerId,
        string userId,
        string title,
        string displayName,
        string position,
        string? comment = null,
        int starRating = 5,
        bool isShowOnHomepage = false,
        string? avatarName = null)
    {
        if (customerId <= 0)
            throw new ArgumentException("CustomerId must be positive.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(position))
            throw new ArgumentException("Position cannot be empty.", nameof(position));
        if (starRating < 1 || starRating > 5)
            throw new ArgumentOutOfRangeException(nameof(starRating), "Star rating must be between 1 and 5.");

        return new CustomerRepresentative
        {
            CustomerId = customerId,
            UserId = userId,
            AvatarName = avatarName,
            Title = title.Trim(),
            DisplayName = displayName.Trim(),
            Position = position.Trim(),
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            StarRating = starRating,
            IsShowOnHomepage = isShowOnHomepage
        };
    }

    public void Update(
        string userId,
        string title,
        string displayName,
        string position,
        string? comment,
        int starRating,
        bool isShowOnHomepage,
        string? avatarName)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(position))
            throw new ArgumentException("Position cannot be empty.", nameof(position));
        if (starRating < 1 || starRating > 5)
            throw new ArgumentOutOfRangeException(nameof(starRating), "Star rating must be between 1 and 5.");

        UserId = userId;
        Title = title.Trim();
        DisplayName = displayName.Trim();
        Position = position.Trim();
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        StarRating = starRating;
        IsShowOnHomepage = isShowOnHomepage;
        AvatarName = avatarName;
    }

    public void SetShowOnHomepage(bool isShowOnHomepage)
    {
        IsShowOnHomepage = isShowOnHomepage;
    }
}
