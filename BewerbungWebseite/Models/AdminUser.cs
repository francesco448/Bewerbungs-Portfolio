using System.ComponentModel.DataAnnotations;

namespace BewerbungsSeite.Models;

public class AdminUser
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}
