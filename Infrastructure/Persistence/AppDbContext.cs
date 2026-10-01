using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Domain.Entities;

namespace PruebaTecnica.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Currency> Currencies => Set<Currency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Currency>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<Currency>()
            .Property(c => c.RateToBase)
            .HasPrecision(18, 4);

        // Sembrado inicial de moneda base (PYG = 1, USD = 7300)
        modelBuilder.Entity<Currency>().HasData(
            new Currency { Id = 1, Code = "PYG", Name = "Guaraní Paraguayo", RateToBase = 1.0m },
            new Currency { Id = 2, Code = "USD", Name = "Dólar Estadounidense", RateToBase = 7300.0m }
        );
    }
}