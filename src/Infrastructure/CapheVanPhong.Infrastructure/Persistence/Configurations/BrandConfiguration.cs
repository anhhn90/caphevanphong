using CapheVanPhong.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapheVanPhong.Infrastructure.Persistence.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(b => b.Slug)
            .IsUnique();

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        builder.Property(b => b.LogoName)
            .HasMaxLength(255);

        builder.Property(b => b.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(b => b.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(b => b.Products)
            .WithOne(p => p.Brand)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
