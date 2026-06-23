using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.TrackInventory).HasDefaultValue(false);
        builder.Property(p => p.StockQuantity).HasDefaultValue(0);
        builder.Property(p => p.LowStockThreshold).HasDefaultValue(0);
        builder.Property(p => p.RowVersion).IsRowVersion();
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.TrackInventory);
        builder.HasIndex(p => p.StockQuantity);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Products_StockQuantity_NonNegative", "[StockQuantity] >= 0");
            t.HasCheckConstraint("CK_Products_LowStockThreshold_NonNegative", "[LowStockThreshold] >= 0");
        });
    }
}
