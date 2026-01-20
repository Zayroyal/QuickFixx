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
    }
}
