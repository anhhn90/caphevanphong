#nullable enable

using CapheVanPhong.Domain.Common;
using CapheVanPhong.Domain.Enums;

namespace CapheVanPhong.Domain.Entities;

public class Order : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string? CustomerEmail { get; private set; }
    public string? DeliveryAddress { get; private set; }
    public string? Notes { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    private Order() { } // EF Core constructor

    public static Order Create(string userId, string customerName, string customerPhone, string? customerEmail = null, string? deliveryAddress = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Tên khách hàng không được để trống", nameof(customerName));
        
        if (string.IsNullOrWhiteSpace(customerPhone))
            throw new ArgumentException("Số điện thoại không được để trống", nameof(customerPhone));

        return new Order
        {
            UserId = userId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            CustomerEmail = customerEmail,
            DeliveryAddress = deliveryAddress,
            Notes = notes,
            TotalAmount = 0,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Số lượng phải lớn hơn 0", nameof(quantity));

        var orderItem = OrderItem.Create(Id, product.Id, product.Name, product.Price, quantity);
        OrderItems.Add(orderItem);
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
        if (status == OrderStatus.Completed)
            CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecalculateTotal()
    {
        TotalAmount = OrderItems.Sum(item => item.Subtotal);
        UpdatedAt = DateTime.UtcNow;
    }
}
