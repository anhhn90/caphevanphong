#nullable enable

using CapheVanPhong.Domain.Common;

namespace CapheVanPhong.Domain.Entities;

public class ProductImage : BaseEntity
{
    public int ProductId { get; private set; }
    public string ImageName { get; private set; } = string.Empty;
    public bool IsMain { get; private set; }
    public int DisplayOrder { get; private set; }

    public Product? Product { get; private set; }

    private ProductImage() { } // EF Core constructor

    public static ProductImage Create(int productId, string imageName, bool isMain, int displayOrder = 0)
    {
        if (productId < 0)
            throw new ArgumentException("ProductId is invalid.", nameof(productId));
        if (string.IsNullOrWhiteSpace(imageName))
            throw new ArgumentException("Product image cannot be empty.", nameof(imageName));

        return new ProductImage
        {
            ProductId = productId,
            ImageName = imageName,
            IsMain = isMain,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void SetMain(bool isMain)
    {
        IsMain = isMain;
        UpdatedAt = DateTime.UtcNow;
    }
}
