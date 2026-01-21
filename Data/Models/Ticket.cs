using System.ComponentModel.DataAnnotations;

namespace BlazorApp2.Data.Models;

public class Ticket
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // NEW: link ticket to the logged-in app user
    public int CreatedByUserId { get; set; }

    
    public string? Description { get; set; }

    // snapshots for history
    public string? CustomerNameSnapshot { get; set; }
    public string? CustomerContactSnapshot { get; set; }
    public string? CustomerEmailSnapshot { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Title { get; set; } = "";
    // Ticket detail fields (Web UI)
    public string? DeviceType { get; set; }
    public string? Diagnostic { get; set; }

    // Cost fields
    public decimal PartsCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal DiagnosticFee { get; set; }
    public decimal TotalCost { get; set; }
}
