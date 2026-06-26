using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

public partial class WhatsAppOrderTrackingNotificationSender(
    AppDbContext db,
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<WhatsAppOrderTrackingNotificationSender> logger) : IOrderTrackingNotificationSender
{
    private const string DefaultGraphApiBaseUrl = "https://graph.facebook.com/v20.0";

    public async Task SendOrderTrackingLinkAsync(
        Order order,
        string? trackingBaseUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var integration = await db.Integrations
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Provider == IntegrationProvider.WhatsApp && i.Enabled, cancellationToken);

            if (integration is null ||
                string.IsNullOrWhiteSpace(integration.PhoneNumberId) ||
                string.IsNullOrWhiteSpace(integration.AccessToken))
            {
                logger.LogInformation("WhatsApp tracking notification skipped because the integration is not configured.");
                return;
            }

            var recipientPhone = NormalizePhone(order.ClientPhone);
            if (string.IsNullOrWhiteSpace(recipientPhone))
            {
                logger.LogInformation("WhatsApp tracking notification skipped because order {OrderId} has no valid phone.", order.Id);
                return;
            }

            var trackingUrl = BuildTrackingUrl(order.Id, trackingBaseUrl);
            if (string.IsNullOrWhiteSpace(trackingUrl))
            {
                logger.LogInformation("WhatsApp tracking notification skipped because no public tracking URL is configured.");
                return;
            }

            var endpoint = $"{GraphApiBaseUrl().TrimEnd('/')}/{integration.PhoneNumberId}/messages";
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(new
                {
                    messaging_product = "whatsapp",
                    to = recipientPhone,
                    type = "text",
                    text = new
                    {
                        preview_url = true,
                        body = BuildMessage(order, trackingUrl)
                    }
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", integration.AccessToken);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "WhatsApp tracking notification failed for order {OrderId}. Status: {StatusCode}. Body: {Body}",
                    order.Id,
                    response.StatusCode,
                    responseBody);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "WhatsApp tracking notification failed for order {OrderId}.", order.Id);
        }
    }

    private string GraphApiBaseUrl() =>
        configuration["WhatsApp:GraphApiBaseUrl"] ?? DefaultGraphApiBaseUrl;

    private string BuildTrackingUrl(Guid orderId, string? trackingBaseUrl)
    {
        var baseUrl = trackingBaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = configuration["OrderTracking:PublicBaseUrl"]
                ?? configuration["Api:BaseUrl"];
        }

        if (string.IsNullOrWhiteSpace(baseUrl) ||
            !Uri.TryCreate(baseUrl.TrimEnd('/'), UriKind.Absolute, out var baseUri))
        {
            return string.Empty;
        }

        return new Uri(baseUri, $"/order-tracking/{orderId}").ToString();
    }

    private static string BuildMessage(Order order, string trackingUrl) =>
        $"Ola, {order.ClientName}! Seu pedido {order.Number} foi recebido. Acompanhe o status por aqui: {trackingUrl}";

    private static string NormalizePhone(string value)
    {
        var digits = DigitsOnlyRegex().Replace(value, string.Empty);

        if (digits.Length is 10 or 11)
        {
            return $"55{digits}";
        }

        return digits.Length >= 12 ? digits : string.Empty;
    }

    [GeneratedRegex(@"\D")]
    private static partial Regex DigitsOnlyRegex();
}
