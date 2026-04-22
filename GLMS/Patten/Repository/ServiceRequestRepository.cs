using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Patterns.Repository
{
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly AppDbContext _context;

        public ServiceRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync()
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> GetByContractIdAsync(int contractId)
        {
            return await _context.ServiceRequests
                .Where(sr => sr.ContractId == contractId)
                .ToListAsync();
        }

        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public async Task AddAsync(ServiceRequest request)
        {
            await _context.ServiceRequests.AddAsync(request);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}