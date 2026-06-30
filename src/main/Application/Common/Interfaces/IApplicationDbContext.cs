using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Estabelecimento> Estabelecimentos { get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<AdditionalGroup> AdditionalGroups { get; }
    DbSet<AdditionalItem> AdditionalItems { get; }
    DbSet<Client> Clients { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderItemAdditional> OrderItemAdditionals { get; }
    DbSet<InventoryMovement> InventoryMovements { get; }
    DbSet<Integration> Integrations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
