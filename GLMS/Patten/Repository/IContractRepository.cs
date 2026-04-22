using GLMS.Web.Models;

namespace GLMS.Web.Patterns.Repository
{
    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllAsync();
        Task<IEnumerable<Contract>> SearchAsync(DateTime? startDate, DateTime? endDate, ContractStatus? status);
        Task<Contract?> GetByIdAsync(int id);
        Task AddAsync(Contract contract);
        Task UpdateAsync(Contract contract);
        Task SaveAsync();
    }
}