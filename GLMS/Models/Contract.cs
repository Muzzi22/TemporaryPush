using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public enum ContractStatus { Draft, Active, Expired, OnHold }

    public class Contract
    {
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required]
        public string ServiceLevel { get; set; } = string.Empty;

        // File handling
        public string? SignedAgreementPath { get; set; }

        // Navigation
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}