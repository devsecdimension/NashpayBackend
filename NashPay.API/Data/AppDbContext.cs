using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NashPay.API.Models;

namespace NashPay.API.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets for all entities
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Ledger> Ledgers { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<Settlement> Settlements { get; set; }
        public DbSet<KYCDocument> KYCDocuments { get; set; }
        public DbSet<WebhookLog> WebhookLogs { get; set; }
        public DbSet<WebhookEndpoint> WebhookEndpoints { get; set; }
        public DbSet<FraudAlert> FraudAlerts { get; set; }
        public DbSet<BankDetails> BankDetails { get; set; }
        public DbSet<CommissionLog> CommissionLogs { get; set; }
        public DbSet<CheckoutSession> CheckoutSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User-Wallet: One-to-One Relationship
            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Transaction Relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Sender)
                .WithMany(u => u.SentTransactions)
                .HasForeignKey(t => t.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Receiver)
                .WithMany(u => u.ReceivedTransactions)
                .HasForeignKey(t => t.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ledger-Transaction Relationship
            modelBuilder.Entity<Ledger>()
                .HasOne(l => l.Transaction)
                .WithMany()
                .HasForeignKey(l => l.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            // User-Ledger: One-to-Many
            modelBuilder.Entity<Ledger>()
                .HasOne(l => l.User)
                .WithMany(u => u.LedgerEntries)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User-ApiKey: One-to-Many
            modelBuilder.Entity<ApiKey>()
                .HasOne(a => a.User)
                .WithMany(u => u.ApiKeys)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User-Settlement: One-to-Many
            modelBuilder.Entity<Settlement>()
                .HasOne(s => s.User)
                .WithMany(u => u.Settlements)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User-KYCDocument: One-to-Many
            modelBuilder.Entity<KYCDocument>()
                .HasOne(k => k.User)
                .WithMany(u => u.KYCDocuments)
                .HasForeignKey(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User-WebhookLog: One-to-Many
            modelBuilder.Entity<WebhookLog>()
                .HasOne(w => w.User)
                .WithMany(u => u.WebhookLogs)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User-FraudAlert: One-to-Many
            modelBuilder.Entity<FraudAlert>()
                .HasOne(f => f.User)
                .WithMany(u => u.FraudAlerts)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // FraudAlert-Transaction Relationship
            modelBuilder.Entity<FraudAlert>()
                .HasOne(f => f.RelatedTransaction)
                .WithMany()
                .HasForeignKey(f => f.RelatedTransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            // User-BankDetails: One-to-Many
            modelBuilder.Entity<BankDetails>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CommissionLog Relationships
            modelBuilder.Entity<CommissionLog>()
                .HasOne(c => c.Transaction)
                .WithMany()
                .HasForeignKey(c => c.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommissionLog>()
                .HasOne(c => c.Merchant)
                .WithMany()
                .HasForeignKey(c => c.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommissionLog>()
                .HasOne(c => c.Admin)
                .WithMany()
                .HasForeignKey(c => c.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // User-WebhookEndpoint: One-to-Many
            modelBuilder.Entity<WebhookEndpoint>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // WebhookEndpoint-WebhookLog: One-to-Many
            // Relationship will be discovered by EF Core from navigation properties and FK on WebhookLog.

            // User-CheckoutSession: One-to-Many
            modelBuilder.Entity<CheckoutSession>()
                .HasOne(c => c.Merchant)
                .WithMany()
                .HasForeignKey(c => c.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique Constraints
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TransactionId)
                .IsUnique();

            modelBuilder.Entity<ApiKey>()
                .HasIndex(a => a.PublicKey)
                .IsUnique();

            modelBuilder.Entity<Settlement>()
                .HasIndex(s => s.SettlementId)
                .IsUnique();
        }
    }
}