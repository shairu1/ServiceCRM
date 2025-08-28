using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServiceCRM.Data;
using ServiceCRM.Models;
using Microsoft.AspNetCore.Identity;

namespace ServiceCRM.Views.Services
{
    public class IndexModel : PageModel
    {
        private readonly ServiceCrmContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ServiceCrmContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // список сервисов
        public List<Service> Services { get; set; } = new();

        // текущий пользователь
        public string? CurrentUserId { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            CurrentUserId = user?.Id;

            Services = await _context.Services
                .Include(s => s.Admin)
                .Include(s => s.Members)
                .ToListAsync();
        }

        // обработчик удаления
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        // обработчик "Присоединиться"
        public async Task<IActionResult> OnPostJoinAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Auth/Login");

            var service = await _context.Services
                .Include(s => s.Members)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            if (!service.Members.Any(m => m.Id.ToString() == user.Id))
            {
                service.Members.Add(new UserService() { UserId = user.Id, ServiceId = service.Id });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
