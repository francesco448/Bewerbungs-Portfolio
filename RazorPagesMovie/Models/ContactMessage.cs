using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public class ContactMessage
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "E-Mail")]
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Telefon")]
    [Phone]
    [StringLength(30)]
    public string? Phone { get; set; }

    [Display(Name = "Betreff")]
    [StringLength(150)]
    public string? Subject { get; set; }

    [Display(Name = "Nachricht")]
    [Required]
    [StringLength(5000)]
    public string Message { get; set; } = string.Empty;

    [Display(Name = "Gesendet am")]
    [DataType(DataType.DateTime)]
    public DateTime SentAt { get; set; } = DateTime.Now;
}
