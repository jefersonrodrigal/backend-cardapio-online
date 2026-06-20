using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class IntegrationConfiguration : IEntityTypeConfiguration<Integration>
{
    public void Configure(EntityTypeBuilder<Integration> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Provider).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(i => i.Provider).IsUnique();

        builder.Property(i => i.ClientId).HasMaxLength(200);
        builder.Property(i => i.ClientSecret).HasMaxLength(500);
        builder.Property(i => i.AccountId).HasMaxLength(200);
        builder.Property(i => i.ApiKey).HasMaxLength(500);
        builder.Property(i => i.AccessToken).HasMaxLength(1000);
        builder.Property(i => i.AppSecret).HasMaxLength(500);
        builder.Property(i => i.VerifyToken).HasMaxLength(200);
        builder.Property(i => i.PhoneNumberId).HasMaxLength(200);
        builder.Property(i => i.WebhookUrl).HasMaxLength(1000);
        builder.Property(i => i.WebhookSecret).HasMaxLength(500);
        builder.Property(i => i.AiProvider).HasMaxLength(50);
        builder.Property(i => i.Model).HasMaxLength(100);
        builder.Property(i => i.AssistantId).HasMaxLength(200);
    }
}
