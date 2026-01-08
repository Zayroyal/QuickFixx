using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Models;

namespace BlazorApp2.Data;
    public class QuickFixDbContext : DbContext
{
    public QuickFixDbContext(DbContextOptions<QuickFixDbContext> options)
        : base(options) { }
    public DbSet<Ticket> Tickets => Set<Ticket>();
}

