
using System.ComponentModel.DataAnnotations;

namespace BlazorApp2.Data.Models;

public class Customer
{
    public int Id { get; set; } // true PK

    // 8-digit date id (yyyyMMdd). Keep this as a "Customer ID #", not the PK.
    public int CustomerIdDate { get; set; }

    [Required] public string Name { get; set; } = "";
    [Required] public string Contact { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
