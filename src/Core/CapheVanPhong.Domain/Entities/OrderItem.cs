#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Subtotal { get; private set; }
    
    public Order? Order { get; private set; }
    public Product? Product { get; private set; }

    private OrderItem() { } // EF Core constructor

    public static OrderItem Create(int orderId, int productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Số lượng phải lớn hơn 0", nameof(quantity));
        
        if (unitPrice <= 0)
            throw new ArgumentException("Đơn giá phải lớn hơn 0", nameof(unitPrice));

        var subtotal = unitPrice * quantity;

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Subtotal = subtotal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Số lượng phải lớn hơn 0", nameof(quantity));

        Quantity = quantity;
        Subtotal = UnitPrice * quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
