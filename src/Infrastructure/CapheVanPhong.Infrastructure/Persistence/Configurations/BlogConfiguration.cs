using CapheVanPhong.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapheVanPhong.Infrastructure.Persistence.Configurations;

public class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.ToTable("Blogs");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(b => b.Slug)
            .IsRequired()
            .HasMaxLength(350);

        builder.HasIndex(b => b.Slug)
            .IsUnique();

        builder.Property(b => b.ImageName)
            .HasMaxLength(255);

        builder.Property(b => b.Introduction)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(b => b.FullContent)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(b => b.IsActive)
            .HasDefaultValue(true);
    }
}
