using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Estabelecimento> Estabelecimentos { get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<Client> Clients { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<InventoryMovement> InventoryMovements { get; }
    DbSet<Integration> Integrations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
