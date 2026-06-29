using System.ComponentModel.DataAnnotations;

namespace QuickFix.Data.Models;

public class Ticket
{
    public int Id { get; set; }

    // Link to the customer's record
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Link to the login account that created the ticket
    public int CreatedByUserId { get; set; }

    public string? Description { get; set; }

    // Customer info at the time the ticket was created
    public string? CustomerNameSnapshot { get; set; }
    public string? CustomerContactSnapshot { get; set; }
    public string? CustomerEmailSnapshot { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Waiting";
    public string TicketNumber { get; set; } = "";

    public string? DeviceType { get; set; }
    public string? Diagnostic { get; set; }

    public decimal PartsCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal DiagnosticFee { get; set; }
    public decimal TotalCost { get; set; }
}