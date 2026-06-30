using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OrderItemAdditionalConfiguration : IEntityTypeConfiguration<OrderItemAdditional>
{
    public void Configure(EntityTypeBuilder<OrderItemAdditional> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.GroupName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Price).HasColumnType("decimal(18,2)");
        builder.Property(a => a.Quantity).HasDefaultValue(1);
        builder.HasOne(a => a.OrderItem)
            .WithMany(i => i.Additionals)
            .HasForeignKey(a => a.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(a => a.OrderItemId);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_OrderItemAdditionals_Quantity_Positive", "[Quantity] >= 1");
        });
    }
}
