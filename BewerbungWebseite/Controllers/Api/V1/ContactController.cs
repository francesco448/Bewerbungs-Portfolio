using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using BewerbungsSeite.Api.V1.Contact;
using BewerbungsSeite.Data;
using BewerbungsSeite.Models;
using BewerbungsSeite.Services;

namespace BewerbungsSeite.Controllers.Api.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class ContactController(RazorPagesMovieContext context, IContactMessageNotificationQueue notificationQueue) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(ContactDto request)
    {
        var contactMessage = new ContactMessage
        {
            Name = request.Name ?? string.Empty,
            Email = request.Email ?? string.Empty,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
            Subject = request.Subject ?? string.Empty,
            Message = request.Message ?? string.Empty
        };

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(contactMessage, new ValidationContext(contactMessage), validationResults, validateAllProperties: true))
        {
            return BadRequest(new { error = "Bitte alle Pflichtfelder korrekt ausfüllen." });
        }

        context.ContactMessage.Add(contactMessage);
        await context.SaveChangesAsync();

        notificationQueue.Enqueue(contactMessage.Id);

        return Ok(new { success = true });
    }
}
