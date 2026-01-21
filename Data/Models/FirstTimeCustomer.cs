using System.ComponentModel.DataAnnotations;

namespace BlazorApp2.Data.Models;

public class FirstTimeCustomer
{
    public int Id { get; set; }

    public int CustomerIdDate { get; set; } // yyyyMMdd

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string Contact { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    // IMPORTANT:
    // Your SQLite table column is named "Created" and is NOT NULL,
    // so the model must match that exact name.
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
