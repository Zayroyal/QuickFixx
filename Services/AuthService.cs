using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Models;

namespace BlazorApp2.Services
{
    public class AuthService
    {
        private readonly QuickFixDbContext _db;

        public AuthService(QuickFixDbContext db)
        {
            _db = db;
        }

        // LOGIN ONLY (no auto-create)
        public async Task<AppUser?> LoginAsync(string email, string password)
        {
            email = (email ?? "").Trim().ToLowerInvariant();
            password = password ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return null;

            if (!PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
                return null;

            user.LastLoginUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return user;
        }

        // REGISTER (create new row)
        public async Task<(bool ok, string error, AppUser? user)> RegisterAsync(string name, string email, string password)
        {
            name = (name ?? "").Trim();
            email = (email ?? "").Trim().ToLowerInvariant();
            password = password ?? "";

            if (string.IsNullOrWhiteSpace(name))
                return (false, "Name is required.", null);

            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required.", null);

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required.", null);

            var exists = await _db.Users.AnyAsync(u => u.Email == email);
            if (exists)
                return (false, "That email is already registered.", null);

            var (hash, salt) = PasswordHasher.Hash(password);

            var user = new AppUser
            {
                Name = name,
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedUtc = DateTime.UtcNow,
                LastLoginUtc = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return (true, "", user);
        }

        public async Task<AppUser?> GetByIdAsync(int id)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        // ----------------------------
        // NEW: SETTINGS ACTIONS
        // ----------------------------

        public async Task ChangeEmailAsync(int userId, string newEmail)
        {
            newEmail = (newEmail ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(newEmail))
                throw new Exception("Email is required.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found.");

            var exists = await _db.Users.AnyAsync(u => u.Email == newEmail && u.Id != userId);
            if (exists)
                throw new Exception("That email is already registered.");

            user.Email = newEmail;
            await _db.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int userId, string newPassword)
        {
            newPassword = (newPassword ?? "").Trim();
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                throw new Exception("Password must be at least 6 characters.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found.");

            var (hash, salt) = PasswordHasher.Hash(newPassword);

            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAccountAsync(int userId)
        {
            // Load user
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found.");

            // Delete that user's tickets + repairs to avoid FK problems
            var tickets = await _db.Tickets
                .Where(t => t.CreatedByUserId == userId)
                .ToListAsync();

            if (tickets.Count > 0)
            {
                var ticketIds = tickets.Select(t => t.Id).ToList();

                var repairs = await _db.Repairs
                    .Where(r => ticketIds.Contains(r.TicketId))
                    .ToListAsync();

                if (repairs.Count > 0)
                    _db.Repairs.RemoveRange(repairs);

                _db.Tickets.RemoveRange(tickets);
            }

            // Finally delete the user row
            _db.Users.Remove(user);

            await _db.SaveChangesAsync();
        }
    }
}
