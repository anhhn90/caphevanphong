#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LogoName { get; private set; }
    public bool IsGoldCustomer { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    private readonly List<CustomerRepresentative> _representatives = new();
    public IReadOnlyList<CustomerRepresentative> Representatives => _representatives.AsReadOnly();

    private Customer() { } // EF Core constructor

    public static Customer Create(
        string name,
        string? description = null,
        string? logoName = null,
        bool isGoldCustomer = false,
        bool isActive = true,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name cannot be empty.", nameof(name));

        return new Customer
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            LogoName = logoName,
            IsGoldCustomer = isGoldCustomer,
            IsActive = isActive,
            DisplayOrder = displayOrder
        };
    }

    public void Update(
        string name,
        string? description,
        string? logoName,
        bool isGoldCustomer,
        bool isActive,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        LogoName = logoName;
        IsGoldCustomer = isGoldCustomer;
        IsActive = isActive;
        DisplayOrder = displayOrder;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void SetGoldCustomer(bool isGoldCustomer)
    {
        IsGoldCustomer = isGoldCustomer;
    }
}
