namespace GLMS.Web.Models
{
    public class ContractNotification
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        // Which user this notification belongs to
        public string UserId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}