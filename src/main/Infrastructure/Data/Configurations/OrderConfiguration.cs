using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Number).HasMaxLength(19).IsRequired();
        builder.Property(o => o.ClientName).HasMaxLength(200).IsRequired();
        builder.Property(o => o.ClientPhone).HasMaxLength(20).IsRequired();
        builder.Property(o => o.Address).HasMaxLength(500);
        builder.Property(o => o.Total).HasColumnType("decimal(18,2)");
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Source).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Note).HasMaxLength(500);
        builder.HasIndex(o => o.Date);
        builder.HasIndex(o => o.Number).IsUnique();
        builder.HasIndex(o => o.Status);
        builder.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
