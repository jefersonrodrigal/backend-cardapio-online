namespace Api.Configuration;

public class AdminAuthOptions
{
    public const string SectionName = "AdminAuth";

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = "CardapioOnline";
    public string JwtAudience { get; set; } = "CardapioOnlineAdmin";
    public string JwtSecret { get; set; } = string.Empty;
    public int TokenExpirationMinutes { get; set; } = 480;
}
