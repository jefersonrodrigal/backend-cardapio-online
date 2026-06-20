using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(20).IsRequired();
        builder.Property(c => c.ZipCode).HasMaxLength(9).IsRequired();
        builder.Property(c => c.Street).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Number).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Neighborhood).HasMaxLength(100).IsRequired();
        builder.Property(c => c.City).HasMaxLength(100).IsRequired();
        builder.Property(c => c.State).HasMaxLength(2).IsRequired();
        builder.Property(c => c.Complement).HasMaxLength(100);
        builder.Property(c => c.PasswordHash).HasMaxLength(500).IsRequired();
        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.Phone).IsUnique();
        builder.HasMany(c => c.Orders).WithOne(o => o.Client).HasForeignKey(o => o.ClientId).OnDelete(DeleteBehavior.SetNull);
    }
}
