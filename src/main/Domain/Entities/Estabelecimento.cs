namespace Domain.Entities;

public class Estabelecimento
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public decimal DeliveryFee { get; set; }
    public bool SendOrderTrackingViaWhatsApp { get; set; }
    public int PreparationTimeMinutes { get; set; } = 30;
    public int DeliverySafetyMarginMinutes { get; set; } = 10;
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TikTokUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
