using System.ComponentModel.DataAnnotations;

namespace BlazorApp2.Data.Models;

public class Repair
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    [Required]
    public string Description { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

