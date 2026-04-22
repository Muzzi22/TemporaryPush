using GLMS.Web.Models;

namespace GLMS.Web.Services
{
    public interface IContractPdfService
    {
        Task<string> GenerateContractPdfAsync(Contract contract);
    }
}