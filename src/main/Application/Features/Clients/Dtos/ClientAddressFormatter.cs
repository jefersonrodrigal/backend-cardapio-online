namespace Application.Features.Clients.Dtos;

public static class ClientAddressFormatter
{
    public static string Format(
        string street,
        string number,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string? complement = null)
    {
        var main = $"{street}, {number} - {neighborhood}";
        var cityState = $"{city} - {state}";
        var zip = string.IsNullOrWhiteSpace(zipCode) ? string.Empty : $"CEP {zipCode}";
        var extra = string.IsNullOrWhiteSpace(complement) ? string.Empty : $" ({complement})";

        return $"{main}, {cityState}{(string.IsNullOrWhiteSpace(zip) ? string.Empty : $", {zip}")}{extra}";
    }
}
