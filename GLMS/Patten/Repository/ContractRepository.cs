using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Patterns.Repository
{
    // Repository Pattern: Abstracts all DB access for Contracts
    public class ContractRepository : IContractRepository
    {
        private readonly AppDbContext _context;

        public ContractRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contract>> GetAllAsync()
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> SearchAsync(DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            // LINQ filtering
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            return await query.ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Contract contract)
        {
            await _context.Contracts.AddAsync(contract);
        }

        public async Task UpdateAsync(Contract contract)
        {
            _context.Contracts.Update(contract);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}