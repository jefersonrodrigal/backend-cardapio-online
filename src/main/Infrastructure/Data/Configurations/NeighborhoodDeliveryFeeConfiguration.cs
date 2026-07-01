using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class NeighborhoodDeliveryFeeConfiguration : IEntityTypeConfiguration<NeighborhoodDeliveryFee>
{
    public void Configure(EntityTypeBuilder<NeighborhoodDeliveryFee> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Neighborhood).HasMaxLength(150).IsRequired();
        builder.Property(n => n.Fee).HasColumnType("decimal(10,2)");
        builder.Property(n => n.IsActive).HasDefaultValue(true);
        builder.HasIndex(n => n.Neighborhood).IsUnique();
    }
}
