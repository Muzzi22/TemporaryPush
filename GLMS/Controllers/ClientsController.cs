using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly AppDbContext _context;

        public ClientsController(AppDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients
                .Include(c => c.Contracts)
                .ToListAsync();
            return View(clients);
        }

        // all users can view client details 
        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return NotFound();
            return View(client);
        }

        
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // admin- save client
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string Name, string ContactDetails, string Region)
        {
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(ContactDetails) ||
                string.IsNullOrWhiteSpace(Region))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            var client = new Client
            {
                Name = Name,
                ContactDetails = ContactDetails,
                Region = Region
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Client '{Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // admin - show form
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // admin- save and delete 
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, string Name, string ContactDetails, string Region)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            client.Name = Name;
            client.ContactDetails = ContactDetails;
            client.Region = Region;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Client updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}