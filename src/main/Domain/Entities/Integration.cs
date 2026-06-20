using Domain.Enums;

namespace Domain.Entities;

public class Integration
{
    public int Id { get; set; }
    public IntegrationProvider Provider { get; set; }
    public bool Enabled { get; set; }

    // OAuth2 / API credentials (iFood, Uber Eats, 99Food)
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    // Merchant/Store/Account identifier (iFood MerchantId, Uber Eats/99Food StoreId,
    // WhatsApp Business Account Id, Anotai Account Id)
    public string AccountId { get; set; } = string.Empty;

    // API key style credentials (Anotai token, AI Agents provider key)
    public string ApiKey { get; set; } = string.Empty;

    // WhatsApp Cloud API
    public string AccessToken { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string VerifyToken { get; set; } = string.Empty;
    public string PhoneNumberId { get; set; } = string.Empty;

    // Webhooks
    public string WebhookUrl { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    // AI Agents
    public string AiProvider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string AssistantId { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
