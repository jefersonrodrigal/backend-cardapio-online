using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class AdditionalGroupConfiguration : IEntityTypeConfiguration<AdditionalGroup>
{
    public void Configure(EntityTypeBuilder<AdditionalGroup> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.MinSelections).HasDefaultValue(0);
        builder.Property(g => g.MaxSelections).HasDefaultValue(1);
        builder.Property(g => g.SortOrder).HasDefaultValue(0);
        builder.HasOne(g => g.Product)
            .WithMany(p => p.AdditionalGroups)
            .HasForeignKey(g => g.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(g => g.ProductId);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_AdditionalGroups_MinSelections_NonNegative", "[MinSelections] >= 0");
            t.HasCheckConstraint("CK_AdditionalGroups_MaxSelections_Positive", "[MaxSelections] >= 1");
            t.HasCheckConstraint("CK_AdditionalGroups_MinMax", "[MinSelections] <= [MaxSelections]");
        });
    }
}
