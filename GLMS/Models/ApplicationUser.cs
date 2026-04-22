using Microsoft.AspNetCore.Identity;

namespace GLMS.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Links this user to a specific client company
        // Null = Admin or unassigned
        public int? ClientId { get; set; }
        public Client? Client { get; set; }
    }
}