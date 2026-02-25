using CapheVanPhong.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapheVanPhong.Infrastructure.Persistence.Configurations;

public class CommercialServiceConfiguration : IEntityTypeConfiguration<CommercialService>
{
    public void Configure(EntityTypeBuilder<CommercialService> builder)
    {
        builder.ToTable("CommercialServices");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Slug)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasIndex(s => s.Slug)
            .IsUnique();

        builder.Property(s => s.Introduction)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.Content)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.IconClass)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ImageName)
            .HasMaxLength(255);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.DisplayOrder)
            .HasDefaultValue(0);
    }
}
