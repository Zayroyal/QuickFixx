using System.ComponentModel.DataAnnotations;

namespace BlazorApp2.Models;

public class CreateTicketFormModel
{
    // Customer
    [Required] public string CustomerName { get; set; } = "";
    [Required] public string Contact { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    // Ticket
    [Required] public string Title { get; set; } = "";
    public string? DeviceType { get; set; }
    public string? Diagnostic { get; set; }

    [Required] public string? Description { get; set; }

    // Costs
    [Range(0, 99999)] public decimal PartsCost { get; set; }
    [Range(0, 99999)] public decimal LaborCost { get; set; }
    [Range(0, 99999)] public decimal DiagnosticFee { get; set; }
}
