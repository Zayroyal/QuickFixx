namespace BlazorApp2.Data.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        public string Name { get; set; } = ""; // ADDED: display name for Welcome message

        public string Email { get; set; } = "";

        public byte[] PasswordHash { get; set; } = System.Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = System.Array.Empty<byte>();

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginUtc { get; set; } = DateTime.UtcNow;
    }
}
