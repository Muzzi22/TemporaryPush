using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    [Authorize]
    public class UserDashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserDashboardController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Get unread notifications for this user
            var notifications = await _context.ContractNotifications
                .Include(n => n.Contract)
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Get active contracts they can view
            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            ViewBag.Notifications = notifications;
            ViewBag.ActiveContracts = activeContracts;
            ViewBag.UserName = user.FullName;

            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var notification = await _context.ContractNotifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == user!.Id);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            var notifications = await _context.ContractNotifications
                .Where(n => n.UserId == user!.Id && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}