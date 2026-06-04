using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrencyService _currencyService;

        public ServiceRequestsController(AppDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _context.ServiceRequests
                .Include(r => r.Contract)
                .ToListAsync();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _context.ServiceRequests
                .Include(r => r.Contract)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound(new { message = "Service request not found" });
            return Ok(request);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var contract = await _context.Contracts.FindAsync(request.ContractId);
            if (contract == null) return NotFound(new { message = "Contract not found" });

            if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
                return BadRequest(new { message = "Cannot create service request for an Expired or On Hold contract." });

            // Currency conversion
            var rate = await _currencyService.GetUsdToZarRateAsync();
            request.ExchangeRateUsed = rate;
            request.CostZAR = request.CostUSD * rate;

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRequestStatusDto dto)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return NotFound(new { message = "Service request not found" });

            request.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(request);
        }
    }

    public class UpdateRequestStatusDto
    {
        public RequestStatus Status { get; set; }
    }
}