using GLMS.Web.Models;

namespace GLMS.Web.Patterns.Repository
{
    public interface IServiceRequestRepository
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<IEnumerable<ServiceRequest>> GetByContractIdAsync(int contractId);
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task AddAsync(ServiceRequest request);
        Task SaveAsync();
    }
}