using GLMS.Web.Models;
using GLMS.Web.Patterns.Repository;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;

namespace GLMS.Web.Controllers
{
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly IServiceRequestRepository _srRepo;
        private readonly IContractRepository _contractRepo;
        private readonly ICurrencyService _currencyService;
        private readonly AppDbContext _context;

        public ServiceRequestsController(
            IServiceRequestRepository srRepo,
            IContractRepository contractRepo,
            ICurrencyService currencyService,
            AppDbContext context)
        {
            _srRepo = srRepo;
            _contractRepo = contractRepo;
            _currencyService = currencyService;
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c!.Client)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        
        public async Task<IActionResult> Create(int? contractId)
        {
            
            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .OrderBy(c => c.Client!.Name)
                .ToListAsync();

            if (!activeContracts.Any())
            {
                TempData["Error"] = "No active contracts found. A service request requires an active contract.";
                return RedirectToAction(nameof(Index));
            }

            var rate = await _currencyService.GetUsdToZarRateAsync();

            ViewBag.ActiveContracts = activeContracts;
            ViewBag.SelectedContractId = contractId;
            ViewBag.ExchangeRate = rate;

            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int contractId, string description, decimal costUSD)
        {
            
            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            var rate = await _currencyService.GetUsdToZarRateAsync();
            ViewBag.ActiveContracts = activeContracts;
            ViewBag.SelectedContractId = contractId;
            ViewBag.ExchangeRate = rate;

            
            if (string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError("", "Description is required.");
                return View();
            }

            if (costUSD <= 0)
            {
                ModelState.AddModelError("", "Cost must be greater than zero.");
                return View();
            }

            
            var contract = await _contractRepo.GetByIdAsync(contractId);
            if (contract == null)
            {
                ModelState.AddModelError("", "Selected contract not found.");
                return View();
            }

           
            if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
            {
                TempData["Error"] = $"Cannot create a service request for a contract with status: {contract.Status}.";
                return RedirectToAction(nameof(Index));
            }

            var costZar = _currencyService.ConvertUsdToZar(costUSD, rate);

            var request = new ServiceRequest
            {
                ContractId = contractId,
                Description = description,
                CostUSD = costUSD,
                CostZAR = costZar,
                ExchangeRateUsed = rate,
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _srRepo.AddAsync(request);
            await _srRepo.SaveAsync();

            TempData["Success"] = "Service request created successfully.";
            return RedirectToAction(nameof(Index));
        }

        
        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(sr => sr.Id == id);

            if (request == null) return NotFound();
            return View(request);
        }
    }
}