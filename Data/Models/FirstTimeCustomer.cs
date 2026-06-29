using System.ComponentModel.DataAnnotations;
namespace BlazorApp2.Data.Models;

    public class FirstTimeCustomer
    {
        public int Id { get; set; }

        // NEW
        public int AppUserId { get; set; }

        public int CustomerIdDate { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Contact { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
