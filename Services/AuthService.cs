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

        public async Task<AppUser?> LoginOrCreateAsync(string email, string password)
        {
            email = (email ?? "").Trim().ToLowerInvariant();
            password = password ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                var (hash, salt) = PasswordHasher.Hash(password);

                user = new AppUser
                {
                    Email = email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    CreatedUtc = DateTime.UtcNow,
                    LastLoginUtc = DateTime.UtcNow
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return user;
            }

            if (!PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
                return null;

            user.LastLoginUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return user;
        }

        public async Task<AppUser?> GetByIdAsync(int id)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
