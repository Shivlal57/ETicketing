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
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
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
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.UnitPrice)
                    .HasPrecision(18, 2);

                entity.Property(o => o.TotalAmount)
                    .HasPrecision(18, 2);

                entity.Property(o => o.Status)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.HasOne(o => o.Ticket)
                    .WithMany()
                    .HasForeignKey(o => o.TicketId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(table =>
                {
                    table.HasCheckConstraint(
                        "CK_Orders_Quantity",
                        "[Quantity] > 0");

                    table.HasCheckConstraint(
                        "CK_Orders_Amounts",
                        "[UnitPrice] > 0 AND [TotalAmount] > 0");
                });
            });

            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Amount)
                    .HasPrecision(18, 2);

                entity.Property(p => p.PaymentMethod)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(p => p.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.HasOne(p => p.Order)
                    .WithMany()
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => new { p.Status, p.ConfirmationDueAt });

                entity.HasIndex(p => p.OrderId).IsUnique().HasFilter("[Status] IN ('Pending', 'Succeeded')");
            });

            modelBuilder.Entity<LedgerEntry>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AccountCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.EntryType)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(e => e.Amount)
                    .HasPrecision(18, 2);

                entity.HasOne(e => e.PaymentTransaction)
                    .WithMany()
                    .HasForeignKey(e => e.PaymentTransactionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new
                {
                    e.PaymentTransactionId,
                    e.AccountCode
                }).IsUnique();

                entity.ToTable(table =>
                {
                    table.HasCheckConstraint(
                        "CK_LedgerEntries_Amount",
                        "[Amount] > 0");

                    table.HasCheckConstraint(
                        "CK_LedgerEntries_EntryType",
                        "[EntryType] IN ('Debit', 'Credit')");

                    table.HasCheckConstraint(
                        "CK_LedgerEntries_AccountCode",
                        "[AccountCode] IN ('CASH', 'TICKET_REVENUE')");
                });
            });

        }
    }
}
