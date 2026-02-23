using CapheVanPhong.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapheVanPhong.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.Property(p => p.Origin)
            .HasMaxLength(255);

        builder.Property(p => p.Model)
            .HasMaxLength(255);

        builder.Property(p => p.Voltage)
            .HasMaxLength(100);

        builder.Property(p => p.Power)
            .HasMaxLength(100);

        builder.Property(p => p.Dimensions)
            .HasMaxLength(255);

        builder.Property(p => p.Weight)
            .HasMaxLength(100);

        builder.Property(p => p.Condition)
            .HasMaxLength(255);

        builder.Property(p => p.WarrantyPeriod)
            .HasMaxLength(255);

        builder.Property(p => p.SalesRegion)
            .HasMaxLength(255);

        builder.Property(p => p.Capacity)
            .HasMaxLength(100);

        builder.Property(p => p.Material)
            .HasMaxLength(255);

        // Brand: 1:N (kept)
        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        // M:N with Categories is configured in ProductCategoryConfiguration

        builder.HasMany(p => p.ProductImages)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
