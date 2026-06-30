using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Estabelecimento> Estabelecimentos => Set<Estabelecimento>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<AdditionalGroup> AdditionalGroups => Set<AdditionalGroup>();
    public DbSet<AdditionalItem> AdditionalItems => Set<AdditionalItem>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemAdditional> OrderItemAdditionals => Set<OrderItemAdditional>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<Integration> Integrations => Set<Integration>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
