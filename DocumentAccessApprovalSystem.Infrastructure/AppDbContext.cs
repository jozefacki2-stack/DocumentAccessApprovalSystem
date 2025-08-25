using DocumentAccessApprovalSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentAccessApprovalSystem.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<User> Users => Set<User>();
        public DbSet<Domain.Entities.Document> Documents => Set<Domain.Entities.Document>();
        public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();
        public DbSet<Decision> Decisions => Set<Decision>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessRequest>()
            .HasOne(ar => ar.Decision)
            .WithOne()
            .HasForeignKey<Decision>(d => d.AccessRequestId)
            .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<AccessRequest>()
            .HasIndex(ar => new { ar.UserId, ar.DocumentId, ar.AccessType, ar.Status });
        }
    }
}
