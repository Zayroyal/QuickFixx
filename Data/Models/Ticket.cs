namespace BlazorApp2.Data.Models;
using System.ComponentModel.DataAnnotations;


public class Ticket
{
    public int ID { get; set; }

    [Required]

    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



}
