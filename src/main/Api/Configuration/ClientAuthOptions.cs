namespace Api.Configuration;

public class ClientAuthOptions
{
    public const string SectionName = "ClientAuth";
    public const string AuthenticationScheme = "ClientJwt";

    public string JwtIssuer { get; set; } = "CardapioOnline";
    public string JwtAudience { get; set; } = "CardapioOnlineClient";
    public int TokenExpirationMinutes { get; set; } = 10080;
}
