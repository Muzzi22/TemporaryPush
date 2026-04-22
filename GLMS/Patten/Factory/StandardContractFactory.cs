using GLMS.Web.Models;

namespace GLMS.Web.Patterns.Factory
{
    // Factory Pattern: Creates contracts with correct defaults
    public class StandardContractFactory : IContractFactory
    {
        public Contract CreateContract(int clientId, DateTime start, DateTime end, string serviceLevel)
        {
            return new Contract
            {
                ClientId = clientId,
                StartDate = start,
                EndDate = end,
                ServiceLevel = serviceLevel,
                Status = ContractStatus.Draft  // Always starts as Draft
            };
        }
    }
}