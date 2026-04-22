using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public enum RequestStatus { Pending, InProgress, Completed, Cancelled }

    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public decimal ExchangeRateUsed { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}