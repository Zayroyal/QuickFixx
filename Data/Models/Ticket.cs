using System.ComponentModel.DataAnnotations;

namespace BlazorApp2.Data.Models;

public class Ticket
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [Required] public string Title { get; set; } = "";
    public string? Description { get; set; }

    // snapshots for history
    public string? CustomerNameSnapshot { get; set; }
    public string? CustomerContactSnapshot { get; set; }
    public string? CustomerEmailSnapshot { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
