using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IOrderTrackingNotificationSender
{
    Task SendOrderTrackingLinkAsync(
        Order order,
        string? trackingBaseUrl,
        CancellationToken cancellationToken = default);
}
