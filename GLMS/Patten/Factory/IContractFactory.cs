using GLMS.Web.Models;

namespace GLMS.Web.Patterns.Factory
{
    public interface IContractFactory
    {
        Contract CreateContract(int clientId, DateTime start, DateTime end, string serviceLevel);
    }
}