using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Patterns.Factory;
using GLMS.Web.Patterns.Observer;
using GLMS.Web.Patterns.Repository;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly IContractRepository _contractRepo;
        private readonly IContractFactory _contractFactory;
        private readonly ContractEventService _eventService;
        private readonly IFileService _fileService;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContractPdfService _pdfService;
        private readonly IWebHostEnvironment _env;

        public ContractsController(
            IContractRepository contractRepo,
            IContractFactory contractFactory,
            ContractEventService eventService,
            IFileService fileService,
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IContractPdfService pdfService,
            IWebHostEnvironment env)
        {
            _contractRepo = contractRepo;
            _contractFactory = contractFactory;
            _eventService = eventService;
            _fileService = fileService;
            _context = context;
            _userManager = userManager;
            _pdfService = pdfService;
            _env = env;
        }

        public async Task<IActionResult> Index(
            DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            IEnumerable<Contract> contracts;
            if (User.IsInRole("Admin"))
            {
                
                contracts = await _contractRepo.SearchAsync(startDate, endDate, status);
            }
            else
            {
                
                var user = await _userManager.GetUserAsync(User);
                if (user?.ClientId == null)
                {
                    // Show empty page with a friendly message instead of error
                    ViewBag.IsAdmin = false;
                    ViewBag.StatusList = Enum.GetValues<ContractStatus>();
                    ViewBag.NotLinked = true;
                    return View(Enumerable.Empty<Contract>());
                }
                contracts = await _context.Contracts
                    .Include(c => c.Client)
                    .Where(c => c.ClientId == user.ClientId)
                    .OrderByDescending(c => c.StartDate)
                    .ToListAsync();
            }
            ViewBag.StatusList = Enum.GetValues<ContractStatus>();
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.SelectedStatus = status;
            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.NotLinked = false;
            return View(contracts);
        }

        // ── Details ───────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _contractRepo.GetByIdAsync(id);
            if (contract == null) return NotFound();

            // Clients can only view their own contracts
            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.ClientId != contract.ClientId)
                    return RedirectToAction("AccessDenied", "Account");
            }

            var loggedInUser = await _userManager.GetUserAsync(User);

            
            var signedUpload = await _context.ClientSignedContracts
                .FirstOrDefaultAsync(s => s.ContractId == id);

            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.SignedUpload = signedUpload;

            return View(contract);
        }

        // ── ADMIN: Create ─────────────────────────────────────
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Clients = await _context.Clients.ToListAsync();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int clientId, DateTime startDate,
            DateTime endDate, string serviceLevel)
        {
            ViewBag.Clients = await _context.Clients.ToListAsync();

            if (string.IsNullOrWhiteSpace(serviceLevel))
            {
                ModelState.AddModelError("", "Service level is required.");
                return View();
            }

            if (endDate <= startDate)
            {
                ModelState.AddModelError("",
                    "End date must be after start date.");
                return View();
            }

            // Factory Pattern
            var contract = _contractFactory.CreateContract(
                clientId, startDate, endDate, serviceLevel);

            await _contractRepo.AddAsync(contract);
            await _contractRepo.SaveAsync();

            // Reload with client details for PDF
            var fullContract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == contract.Id);

            if (fullContract != null)
            {
                // Generate PDF automatically
                var pdfPath = await _pdfService
                    .GenerateContractPdfAsync(fullContract);

                fullContract.SignedAgreementPath = pdfPath;
                await _contractRepo.UpdateAsync(fullContract);
                await _contractRepo.SaveAsync();

                // Notify client users
                await NotifyUsersAsync(
                    fullContract,
                    $"A new contract (#{fullContract.Id} — " +
                    $"{fullContract.ServiceLevel}) has been created for " +
                    $"your account. Please download it, sign it, and " +
                    $"upload your signed copy.",
                    notifyClients: true,
                    notifyAdmins: false);
            }

            TempData["Success"] =
                "Contract created, PDF generated, and client notified.";
            return RedirectToAction(nameof(Index));
        }

        // ── ADMIN: Update Status ──────────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id, ContractStatus newStatus)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null) return NotFound();

            var oldStatus = contract.Status;
            contract.Status = newStatus;

            await _contractRepo.UpdateAsync(contract);
            await _contractRepo.SaveAsync();

            await _eventService.NotifyStatusChangedAsync(
                contract, oldStatus, newStatus);

            // Notify clients when contract becomes active
            if (newStatus == ContractStatus.Active &&
                oldStatus != ContractStatus.Active)
            {
                await NotifyUsersAsync(
                    contract,
                    $"Your contract (#{contract.Id} — " +
                    $"{contract.ServiceLevel}) is now Active. " +
                    $"You can now raise service requests against it.",
                    notifyClients: true,
                    notifyAdmins: false);
            }

            TempData["Success"] = $"Status updated to {newStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Download generated contract PDF ───────────────────
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _contractRepo.GetByIdAsync(id);

            if (contract == null ||
                string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound();

            // Clients can only download their own contract
            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.ClientId != contract.ClientId)
                    return RedirectToAction("AccessDenied", "Account");
            }

            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                contract.SignedAgreementPath);

            if (!System.IO.File.Exists(fullPath)) return NotFound();

            return PhysicalFile(
                fullPath,
                "application/pdf",
                $"Contract_GLMS{id:D5}.pdf");
        }

        // ── CLIENT: Upload signed contract ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSignedContract(
            int contractId, IFormFile signedFile)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == contractId);

            if (contract == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Ensure client owns this contract
            if (!User.IsInRole("Admin") &&
                user.ClientId != contract.ClientId)
                return RedirectToAction("AccessDenied", "Account");

            if (signedFile == null || signedFile.Length == 0)
            {
                TempData["Error"] = "Please select a PDF file.";
                return RedirectToAction(nameof(Details),
                    new { id = contractId });
            }

            if (!_fileService.IsValidPdf(signedFile))
            {
                TempData["Error"] = "Only PDF files are allowed.";
                return RedirectToAction(nameof(Details),
                    new { id = contractId });
            }

            // Save file
            var folderPath = Path.Combine(
                _env.WebRootPath, "contracts", "signed");
            Directory.CreateDirectory(folderPath);

            var fileName = $"Signed_{contractId}_{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await signedFile.CopyToAsync(stream);

            var relativePath = Path.Combine(
                "contracts", "signed", fileName);

            // Save or replace existing record
            var existing = await _context.ClientSignedContracts
                .FirstOrDefaultAsync(s => s.ContractId == contractId);

            if (existing != null)
            {
                existing.SignedFilePath = relativePath;
                existing.UploadedAt = DateTime.UtcNow;
                existing.UserId = user.Id;
            }
            else
            {
                _context.ClientSignedContracts.Add(
                    new ClientSignedContract
                    {
                        ContractId = contractId,
                        UserId = user.Id,
                        SignedFilePath = relativePath,
                        UploadedAt = DateTime.UtcNow
                    });
            }

            await _context.SaveChangesAsync();

            // Notify all admins
            await NotifyUsersAsync(
                contract,
                $"Client '{contract.Client?.Name}' has uploaded their " +
                $"signed contract for Contract #" +
                $"{contractId} ({contract.ServiceLevel}). " +
                $"Please review it.",
                notifyClients: false,
                notifyAdmins: true);

            TempData["Success"] =
                "Your signed contract has been uploaded successfully. " +
                "Our team has been notified.";

            return RedirectToAction(nameof(Details),
                new { id = contractId });
        }

        // ── ADMIN: Download client signed contract ────────────
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadSignedContract(int id)
        {
            var signed = await _context.ClientSignedContracts
                .FirstOrDefaultAsync(s => s.Id == id);

            if (signed == null) return NotFound();

            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                signed.SignedFilePath);

            if (!System.IO.File.Exists(fullPath)) return NotFound();

            return PhysicalFile(
                fullPath,
                "application/pdf",
                $"SignedContract_{signed.ContractId}.pdf");
        }

        // ── Notification helper ───────────────────────────────
        private async Task NotifyUsersAsync(
            Contract contract,
            string message,
            bool notifyClients,
            bool notifyAdmins)
        {
            var allUsers = _userManager.Users.ToList();

            foreach (var u in allUsers)
            {
                bool isAdmin = await _userManager.IsInRoleAsync(u, "Admin");

                bool shouldNotify =
                    (notifyAdmins && isAdmin) ||
                    (notifyClients && !isAdmin &&
                     u.ClientId == contract.ClientId);

                if (shouldNotify)
                {
                    _context.ContractNotifications.Add(
                        new ContractNotification
                        {
                            ContractId = contract.Id,
                            UserId = u.Id,
                            Message = message,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}