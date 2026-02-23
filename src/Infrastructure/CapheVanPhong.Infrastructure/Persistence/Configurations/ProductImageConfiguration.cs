#nullable enable

using CapheVanPhong.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapheVanPhong.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(i => i.DisplayOrder)
            .HasDefaultValue(0);

        builder.HasIndex(i => new { i.ProductId, i.DisplayOrder });
        builder.HasIndex(i => new { i.ProductId, i.IsMain })
            .IsUnique()
            .HasFilter("[IsMain] = 1");
    }
}
