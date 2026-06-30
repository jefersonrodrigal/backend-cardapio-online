using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class AdditionalItemConfiguration : IEntityTypeConfiguration<AdditionalItem>
{
    public void Configure(EntityTypeBuilder<AdditionalItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Price).HasColumnType("decimal(18,2)");
        builder.Property(i => i.IsAvailable).HasDefaultValue(true);
        builder.Property(i => i.SortOrder).HasDefaultValue(0);
        builder.HasOne(i => i.Group)
            .WithMany(g => g.Items)
            .HasForeignKey(i => i.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(i => i.GroupId);
    }
}
