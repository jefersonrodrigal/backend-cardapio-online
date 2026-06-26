using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class EstabelecimentoConfiguration : IEntityTypeConfiguration<Estabelecimento>
{
    public void Configure(EntityTypeBuilder<Estabelecimento> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.LogoUrl).HasMaxLength(5000);
        builder.Property(e => e.Category).HasMaxLength(50);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.Whatsapp).HasMaxLength(20);
        builder.Property(e => e.DeliveryFee).HasColumnType("decimal(18,2)");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetimeoffset");
    }
}
