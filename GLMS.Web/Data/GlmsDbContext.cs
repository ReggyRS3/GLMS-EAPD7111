using GLMS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Data
{
    public class GlmsDbContext : DbContext
    {
        public GlmsDbContext(DbContextOptions<GlmsDbContext> options) : base(options) { }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
        public DbSet<ContractAuditLog> ContractAuditLogs => Set<ContractAuditLog>();
        public DbSet<ManagerAlert> ManagerAlerts => Set<ManagerAlert>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Save statuses as text ("Active") instead of numbers (1) so the database is readable
            modelBuilder.Entity<Contract>()
                .Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<ServiceRequest>()
                .Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Sample data so the search and workflow can be tested straight away
            modelBuilder.Entity<Client>().HasData(
                new Client { ClientId = 1, Name = "Kalahari Freight", ContactEmail = "ops@kalaharifreight.co.za", ContactPhone = "0115550101", Region = "Africa" },
                new Client { ClientId = 2, Name = "Rhine Cargo GmbH", ContactEmail = "info@rhinecargo.de", ContactPhone = "+4930555012", Region = "Europe" }
            );

            modelBuilder.Entity<Contract>().HasData(
                new Contract { ContractId = 1, ClientId = 1, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = ContractStatus.Active, ServiceLevel = "Premium" },
                new Contract { ContractId = 2, ClientId = 1, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 12, 31), Status = ContractStatus.Expired, ServiceLevel = "Standard" },
                new Contract { ContractId = 3, ClientId = 2, StartDate = new DateTime(2026, 3, 1), EndDate = new DateTime(2027, 2, 28), Status = ContractStatus.OnHold, ServiceLevel = "Express" }
            );
        }
    }
}