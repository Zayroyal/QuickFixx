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

    private const decimal HARD_DIAGNOSTIC_FEE = 25m;

    // -------------------------------------------------
    // CREATE BASIC TICKET (no costs)
    // -------------------------------------------------
    public async Task<int> CreateTicketAsync(
        int createdByUserId,
        string customerName,
        string customerContact,
        string customerEmail,
        string? description)
    {
        customerEmail = customerEmail.Trim().ToLowerInvariant();

        await using var tx = await _db.Database.BeginTransactionAsync();

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == customerEmail);

        var firstTime = customer == null
            ? await _db.FirstTimeCustomers.FirstOrDefaultAsync(c => c.Email == customerEmail)
            : null;

        int? linkCustomerId = null;

        if (customer != null)
        {
            linkCustomerId = customer.Id;
        }
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
        }
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

        var now = DateTime.UtcNow;

        var ticket = new Ticket
        {
            CreatedByUserId = createdByUserId,
            CustomerId = linkCustomerId,
            Description = description,
            CustomerNameSnapshot = customerName,
            CustomerContactSnapshot = customerContact,
            CustomerEmailSnapshot = customerEmail,
            Status = "Waiting",
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        ticket.TicketNumber = $"QF-{ticket.Id:D4}";
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

    // -------------------------------------------------
    // DELETE TICKET (user-owned only)
    // -------------------------------------------------
    public async Task DeleteTicketAsync(int ticketId, int requestingUserId)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
            throw new Exception("Ticket not found.");

        if (ticket.CreatedByUserId != requestingUserId)
            throw new Exception("Not authorized to delete this ticket.");

        var repairs = await _db.Repairs
            .Where(r => r.TicketId == ticketId)
            .ToListAsync();

        if (repairs.Any())
            _db.Repairs.RemoveRange(repairs);

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
    }

    // -------------------------------------------------
    // USER TICKET QUERIES
    // -------------------------------------------------
    public Task<List<Ticket>> GetUserCurrentTicketsAsync(int userId)
        => _db.Tickets
            .Where(t => t.CreatedByUserId == userId && t.Status != "Completed")
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();

    public Task<List<Ticket>> GetUserOldTicketsAsync(int userId)
        => _db.Tickets
            .Where(t => t.CreatedByUserId == userId && t.Status == "Completed")
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();

    // -------------------------------------------------
    // ADMIN / GENERAL QUERY
    // -------------------------------------------------
    public Task<List<Ticket>> GetTicketsAsync()
        => _db.Tickets
            .Include(t => t.Customer)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

    // -------------------------------------------------
    // CREATE TICKET WITH COSTS
    // -------------------------------------------------
    public async Task<int> CreateTicketWithCostAsync(
        int createdByUserId,
        string customerName,
        string customerContact,
        string customerEmail,
        string? description,
        string? deviceType,
        string? diagnostic,
        decimal partsCost,
        decimal laborCost)
    {
        customerEmail = customerEmail.Trim().ToLowerInvariant();

        await using var tx = await _db.Database.BeginTransactionAsync();

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == customerEmail);

        var firstTime = customer == null
            ? await _db.FirstTimeCustomers.FirstOrDefaultAsync(c => c.Email == customerEmail)
            : null;

        int? linkCustomerId = null;

        if (customer != null)
        {
            linkCustomerId = customer.Id;
        }
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
        }
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

        var diagnosticFee = HARD_DIAGNOSTIC_FEE;
        var totalCost = partsCost + laborCost + diagnosticFee;
        var now = DateTime.UtcNow;

        var ticket = new Ticket
        {
            CreatedByUserId = createdByUserId,
            CustomerId = linkCustomerId,
            Description = description,
            DeviceType = deviceType,
            Diagnostic = diagnostic,
            PartsCost = partsCost,
            LaborCost = laborCost,
            DiagnosticFee = diagnosticFee,
            TotalCost = totalCost,
            CustomerNameSnapshot = customerName,
            CustomerContactSnapshot = customerContact,
            CustomerEmailSnapshot = customerEmail,
            Status = "Waiting",
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        ticket.TicketNumber = $"QF-{ticket.Id:D4}";
        ticket.UpdatedAt = DateTime.UtcNow;
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
