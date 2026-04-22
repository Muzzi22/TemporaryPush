using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Models;

namespace GLMS.Web.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Contract> Contracts { get; set; } = null!;
        public DbSet<ServiceRequest> ServiceRequests { get; set; } = null!;
        public DbSet<ContractNotification> ContractNotifications { get; set; } = null!;
        public DbSet<ClientSignedContract> ClientSignedContracts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Contract)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.CostUSD).HasPrecision(18, 2);
            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.CostZAR).HasPrecision(18, 2);
            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.ExchangeRateUsed).HasPrecision(18, 6);

            // User → Client relationship
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Client)
                .WithMany()
                .HasForeignKey(u => u.ClientId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }
}