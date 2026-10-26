using ETicketing.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETicketing.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Ticket> Tickets => Set<Ticket>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Name)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(t => t.Name)
                    .IsUnique();

                entity.Property(t => t.Price)
                    .HasPrecision(18, 2);

                entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.Property(t => t.RowVersion)
                    .IsRowVersion();

                entity.ToTable(table =>
                {
                    table.HasCheckConstraint(
                        "CK_Tickets_Price",
                        "[Price] > 0");

                    table.HasCheckConstraint(
                        "CK_Tickets_Quota",
                        "[TotalQuota] >= 0 AND " +
                        "[AvailableQuota] >= 0 AND " +
                        "[AvailableQuota] <= [TotalQuota]");
                });

                entity.HasData(
                    new Ticket
                    {
                        Id = 1,
                        Name = "Gold",
                        Price = 100m,
                        TotalQuota = 100,
                        AvailableQuota = 100,
                        IsActive = true
                    },
                    new Ticket
                    {
                        Id = 2,
                        Name = "Premium",
                        Price = 200m,
                        TotalQuota = 200,
                        AvailableQuota = 200,
                        IsActive = true
                    },
                    new Ticket
                    {
                        Id = 3,
                        Name = "VIP",
                        Price = 500m,
                        TotalQuota = 50,
                        AvailableQuota = 50,
                        IsActive = true
                    }
                );
            });
        }
    }
}
