namespace GLMS.Web.Models
{
    public class ClientSignedContract
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public Contract? Contract { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public string SignedFilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}