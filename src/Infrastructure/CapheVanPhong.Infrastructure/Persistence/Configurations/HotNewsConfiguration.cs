using CapheVanPhong.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapheVanPhong.Infrastructure.Persistence.Configurations;

public class HotNewsConfiguration : IEntityTypeConfiguration<HotNews>
{
    public void Configure(EntityTypeBuilder<HotNews> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(h => h.Content)
            .IsRequired();

        builder.Property(h => h.ImageName)
            .HasMaxLength(255);

        builder.Property(h => h.IsActive)
            .HasDefaultValue(true);
    }
}
