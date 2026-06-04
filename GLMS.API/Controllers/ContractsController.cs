using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContractsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ContractStatus? status,
                                                 [FromQuery] DateTime? startDate,
                                                 [FromQuery] DateTime? endDate)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);
            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            var contracts = await query.ToListAsync();
            return Ok(contracts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null) return NotFound(new { message = "Contract not found" });
            return Ok(contract);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Contract contract)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound(new { message = "Contract not found" });

            contract.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(contract);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Contract contract)
        {
            if (id != contract.Id) return BadRequest(new { message = "ID mismatch" });
            _context.Entry(contract).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound(new { message = "Contract not found" });
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class UpdateStatusDto
    {
        public ContractStatus Status { get; set; }
    }
}