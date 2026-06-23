using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Reason).HasMaxLength(300);
        builder.HasIndex(m => m.ProductId);
        builder.HasIndex(m => m.OrderId);
        builder.HasIndex(m => m.CreatedAt);
        builder.HasOne(m => m.Product)
            .WithMany(p => p.InventoryMovements)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Order)
            .WithMany()
            .HasForeignKey(m => m.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_InventoryMovements_Quantity_Positive", "[Quantity] > 0");
            t.HasCheckConstraint("CK_InventoryMovements_BalanceBefore_NonNegative", "[BalanceBefore] >= 0");
            t.HasCheckConstraint("CK_InventoryMovements_BalanceAfter_NonNegative", "[BalanceAfter] >= 0");
        });
    }
}
