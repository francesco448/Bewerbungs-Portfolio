using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages
{
    public class BewerbungModel : PageModel
    {
        private readonly RazorPagesMovieContext _context;

        public BewerbungModel(RazorPagesMovieContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string? Phone { get; set; }

        [BindProperty]
        public string? Subject { get; set; }

        [BindProperty]
        public string Message { get; set; } = string.Empty;

        public bool ContactFormSubmitted { get; set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var contactMessage = new ContactMessage
            {
                Name = Name,
                Email = Email,
                Phone = Phone,
                Subject = Subject,
                Message = Message
            };

            if (!TryValidateModel(contactMessage, nameof(ContactMessage)))
            {
                return Page();
            }

            _context.ContactMessage.Add(contactMessage);
            await _context.SaveChangesAsync();

            ContactFormSubmitted = true;
            return Page();
        }
    }
}
