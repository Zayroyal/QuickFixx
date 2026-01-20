using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace BlazorApp2.Services
{
    public static class PasswordHasher
    {
        public static (byte[] hash, byte[] salt) Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                100_000,
                32);

            return (hash, salt);
        }

        public static bool Verify(string password, byte[] hash, byte[] salt)
        {
            byte[] test = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                100_000,
                32);

            return CryptographicOperations.FixedTimeEquals(test, hash);
        }
    }
}
