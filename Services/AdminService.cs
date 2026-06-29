using BlazorApp2.Data;
using BlazorApp2.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services
{
    public class AdminService
    {
        private readonly QuickFixDbContext _db;

        public AdminService(QuickFixDbContext db)
        {
            _db = db;
        }

        // =====================================================
        // DASHBOARD
        // =====================================================

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            return new DashboardStats
            {
                ActiveTickets = await _db.Tickets
                    .CountAsync(t => t.Status != "Completed"),

                WaitingTickets = await _db.Tickets
                    .CountAsync(t => t.Status == "Waiting"),

                InProgressTickets = await _db.Tickets
                    .CountAsync(t => t.Status == "In Progress"),

                CompletedToday = await _db.Tickets
                    .CountAsync(t =>
                        t.Status == "Completed" &&
                        t.UpdatedAt.Date == DateTime.UtcNow.Date),

                FirstTimeCustomers = await _db.FirstTimeCustomers.CountAsync(),

                ReturningCustomers = await _db.Customers.CountAsync(),

                Revenue = await _db.Tickets
                    .Where(t => t.Status == "Completed")
                    .SumAsync(t => t.TotalCost)
            };
        }

        // =====================================================
        // ACTIVE TICKETS
        // =====================================================

        public async Task<List<Ticket>> GetActiveTicketsAsync()
        {
            return await _db.Tickets
                .Where(t => t.Status != "Completed")
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Ticket?> GetTicketAsync(int id)
        {
            return await _db.Tickets
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            ticket.UpdatedAt = DateTime.UtcNow;

            _db.Tickets.Update(ticket);

            await _db.SaveChangesAsync();
        }

        public async Task DeleteTicketAsync(int id)
        {
            var ticket = await _db.Tickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return;

            var repairs = await _db.Repairs
                .Where(r => r.TicketId == id)
                .ToListAsync();

            if (repairs.Any())
                _db.Repairs.RemoveRange(repairs);

            _db.Tickets.Remove(ticket);

            await _db.SaveChangesAsync();
        }

        // =====================================================
        // COMPLETED TICKETS
        // =====================================================

        public async Task<List<Ticket>> GetCompletedTicketsAsync()
        {
            return await _db.Tickets
                .Where(t => t.Status == "Completed")
                .OrderByDescending(t => t.UpdatedAt)
                .ToListAsync();
        }

        // =====================================================
        // CUSTOMERS
        // =====================================================

        public async Task<List<Customer>> GetCustomersAsync()
        {
            return await _db.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<FirstTimeCustomer>> GetFirstTimeCustomersAsync()
        {
            return await _db.FirstTimeCustomers
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }

    // =========================================================
    // Dashboard Statistics Model
    // =========================================================

    public class DashboardStats
    {
        public int ActiveTickets { get; set; }

        public int WaitingTickets { get; set; }

        public int InProgressTickets { get; set; }

        public int CompletedToday { get; set; }

        public int FirstTimeCustomers { get; set; }

        public int ReturningCustomers { get; set; }

        public decimal Revenue { get; set; }
    }
}