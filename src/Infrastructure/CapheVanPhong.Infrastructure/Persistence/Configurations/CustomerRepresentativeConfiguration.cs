using CapheVanPhong.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapheVanPhong.Infrastructure.Persistence.Configurations;

public class CustomerRepresentativeConfiguration : IEntityTypeConfiguration<CustomerRepresentative>
{
    public void Configure(EntityTypeBuilder<CustomerRepresentative> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(r => r.AvatarName)
            .HasMaxLength(255);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Position)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Comment)
            .HasMaxLength(2000);

        builder.Property(r => r.StarRating)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(r => r.IsShowOnHomepage)
            .HasDefaultValue(false);

        // FK to AspNetUsers — Restrict delete (don't allow deleting a user who is a representative)
        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one user can only be a representative once per company
        builder.HasIndex(r => new { r.CustomerId, r.UserId })
            .IsUnique();
    }
}
