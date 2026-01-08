using BlazorApp2.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Data;

public class QuickFixDbContext : DbContext
{
    public QuickFixDbContext(DbContextOptions<QuickFixDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Repair> Repairs => Set<Repair>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<FirstTimeCustomer> FirstTimeCustomers => Set<FirstTimeCustomer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<FirstTimeCustomer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Customer)
            .WithMany()
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Repair>()
            .HasOne(r => r.Ticket)
            .WithMany()
            .HasForeignKey(r => r.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
