using BlazorApp2.Data;
using BlazorApp2.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services;

public class TicketServices
{
    private readonly QuickFixDbContext _db;

    public TicketServices(QuickFixDbContext db)
    {
        _db = db;
    }

    private static int TodayCustomerIdDate()
        => int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));

    /// <summary>
    /// Rule:
    /// - 1st ticket with a new email -> goes into FirstTimeCustomers only (no Customers row yet)
    /// - 2nd ticket with same email -> promote/move to Customers, remove from FirstTimeCustomers
    /// </summary>
    public async Task<int> CreateTicketAsync(
        int createdByUserId, // NEW: logged-in user id
        string customerName,
        string customerContact,
        string customerEmail,
        string title,
        string? description)
    {
        customerEmail = customerEmail.Trim().ToLowerInvariant();

        await using var tx = await _db.Database.BeginTransactionAsync();

        // 1) Check Customers first
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == customerEmail);

        // 2) If not in Customers, check FirstTimeCustomers
        var firstTime = customer == null
            ? await _db.FirstTimeCustomers.FirstOrDefaultAsync(c => c.Email == customerEmail)
            : null;

        int? linkCustomerId = null;

        // CASE A: already a Customer
        if (customer != null)
        {
            linkCustomerId = customer.Id;
        }
        // CASE B: first time seeing this email -> create FirstTimeCustomer only
        else if (firstTime == null)
        {
            firstTime = new FirstTimeCustomer
            {
                CustomerIdDate = TodayCustomerIdDate(),
                Name = customerName,
                Contact = customerContact,
                Email = customerEmail
            };

            _db.FirstTimeCustomers.Add(firstTime);
            await _db.SaveChangesAsync();

            // Not promoted yet
            linkCustomerId = null;
        }
        // CASE C: email exists in FirstTimeCustomers -> 2nd ticket -> promote to Customers
        else
        {
            // Keep latest contact info
            firstTime.Name = customerName;
            firstTime.Contact = customerContact;
            await _db.SaveChangesAsync();

            var promoted = new Customer
            {
                CustomerIdDate = firstTime.CustomerIdDate,
                Name = firstTime.Name,
                Contact = firstTime.Contact,
                Email = firstTime.Email
            };

            _db.Customers.Add(promoted);
            await _db.SaveChangesAsync();

            _db.FirstTimeCustomers.Remove(firstTime);
            await _db.SaveChangesAsync();

            linkCustomerId = promoted.Id;
        }

        // 3) Create ticket (link only if customer exists/promoted)
        var ticket = new Ticket
        {
            CreatedByUserId = createdByUserId, // NEW: store the logged-in user id
            CustomerId = linkCustomerId,
            Title = title,
            Description = description,
            CustomerNameSnapshot = customerName,
            CustomerContactSnapshot = customerContact,
            CustomerEmailSnapshot = customerEmail
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        // 4) Create repair record from description (optional)
        if (!string.IsNullOrWhiteSpace(description))
        {
            _db.Repairs.Add(new Repair
            {
                TicketId = ticket.Id,
                Description = description
            });
            await _db.SaveChangesAsync();
        }

        await tx.CommitAsync();
        return ticket.Id;
    }

    public Task<List<Ticket>> GetTicketsAsync()
        => _db.Tickets
              .Include(t => t.Customer)
              .OrderByDescending(t => t.CreatedAt)
              .ToListAsync();

    public async Task<int> CreateTicketWithCostAsync(
        int createdByUserId, // NEW: logged-in user id
        string customerName,
        string customerContact,
        string customerEmail,
        string title,
        string? description,
        string? deviceType,
        string? diagnostic,
        decimal partsCost,
        decimal laborCost,
        decimal diagnosticFee,
        decimal totalCost)
    {
        customerEmail = customerEmail.Trim().ToLowerInvariant();

        await using var tx = await _db.Database.BeginTransactionAsync();

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == customerEmail);

        var firstTime = customer == null
            ? await _db.FirstTimeCustomers.FirstOrDefaultAsync(c => c.Email == customerEmail)
            : null;

        int? linkCustomerId = null;

        // A) already a Customer
        if (customer != null)
        {
            linkCustomerId = customer.Id;
        }
        // B) first time seeing this email -> create FirstTimeCustomer only
        else if (firstTime == null)
        {
            firstTime = new FirstTimeCustomer
            {
                CustomerIdDate = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd")),
                Name = customerName,
                Contact = customerContact,
                Email = customerEmail
            };

            _db.FirstTimeCustomers.Add(firstTime);
            await _db.SaveChangesAsync();

            linkCustomerId = null;
        }
        // C) email exists in FirstTimeCustomers -> 2nd ticket -> promote to Customers
        else
        {
            firstTime.Name = customerName;
            firstTime.Contact = customerContact;
            await _db.SaveChangesAsync();

            var promoted = new Customer
            {
                CustomerIdDate = firstTime.CustomerIdDate,
                Name = firstTime.Name,
                Contact = firstTime.Contact,
                Email = firstTime.Email
            };

            _db.Customers.Add(promoted);
            await _db.SaveChangesAsync();

            _db.FirstTimeCustomers.Remove(firstTime);
            await _db.SaveChangesAsync();

            linkCustomerId = promoted.Id;
        }

        var ticket = new Ticket
        {
            CreatedByUserId = createdByUserId, // NEW: store the logged-in user id
            CustomerId = linkCustomerId,

            Title = title,
            Description = description,

            DeviceType = deviceType,
            Diagnostic = diagnostic,

            PartsCost = partsCost,
            LaborCost = laborCost,
            DiagnosticFee = diagnosticFee,
            TotalCost = totalCost,

            // keep snapshots (your current flow)
            CustomerNameSnapshot = customerName,
            CustomerContactSnapshot = customerContact,
            CustomerEmailSnapshot = customerEmail
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(description))
        {
            _db.Repairs.Add(new Repair
            {
                TicketId = ticket.Id,
                Description = description
            });
            await _db.SaveChangesAsync();
        }

        await tx.CommitAsync();
        return ticket.Id;
    }
}
