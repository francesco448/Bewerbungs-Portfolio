using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public class AdminUser
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}
