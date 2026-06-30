using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Orders;

public class OrderDelayMonitorService(
    IServiceScopeFactory scopeFactory,
    ILogger<OrderDelayMonitorService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await MarkExpiredOrdersAsDelayedAsync(stoppingToken);

        using var timer = new PeriodicTimer(CheckInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await MarkExpiredOrdersAsDelayedAsync(stoppingToken);
        }
    }

    private async Task MarkExpiredOrdersAsDelayedAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.UtcNow;

            var delayedOrders = await db.Orders
                .Where(o =>
                    o.EstimatedDeliveryDeadlineAt.HasValue &&
                    o.EstimatedDeliveryDeadlineAt.Value <= now &&
                    o.MarkedDelayedAt == null &&
                    o.Status != OrderStatus.EmAtraso &&
                    o.Status != OrderStatus.Entregue &&
                    o.Status != OrderStatus.Cancelado)
                .ToListAsync(ct);

            if (delayedOrders.Count == 0)
            {
                return;
            }

            foreach (var order in delayedOrders)
            {
                order.Status = OrderStatus.EmAtraso;
                order.MarkedDelayedAt = now;
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("{Count} order(s) marked as delayed.", delayedOrders.Count);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while marking delayed orders.");
        }
    }
}
