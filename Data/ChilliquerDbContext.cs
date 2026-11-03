using ChilliquerAspNetCore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Data;

public class ChilliquerDbContext : DbContext
{
    public ChilliquerDbContext(DbContextOptions<ChilliquerDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<DiscountPolicy> DiscountPolicies => Set<DiscountPolicy>();
    public DbSet<CancellationPolicy> CancellationPolicies => Set<CancellationPolicy>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<MemberCard> MemberCards => Set<MemberCard>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Deposit> Deposits => Set<Deposit>();
    public DbSet<DepositNotification> DepositNotifications => Set<DepositNotification>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().ToTable("Roles").HasKey(x => x.RoleId);
        modelBuilder.Entity<UserAccount>().ToTable("UserAccounts").HasKey(x => x.UserId);
        modelBuilder.Entity<Manufacturer>().ToTable("Manufacturers").HasKey(x => x.ManufacturerId);
        modelBuilder.Entity<Supplier>().ToTable("Suppliers").HasKey(x => x.SupplierId);
        modelBuilder.Entity<ProductCategory>().ToTable("ProductCategories").HasKey(x => x.CategoryId);
        modelBuilder.Entity<Product>().ToTable("Products", tb => tb.HasTrigger("trg_Products")).HasKey(x => x.ProductId);
        modelBuilder.Entity<WarehouseLocation>().ToTable("WarehouseLocations").HasKey(x => x.LocationId);
        modelBuilder.Entity<StockBatch>().ToTable("StockBatches", tb => tb.HasTrigger("trg_StockBatches")).HasKey(x => x.BatchId);
        modelBuilder.Entity<DiscountPolicy>().ToTable("DiscountPolicies").HasKey(x => x.PolicyId);
        modelBuilder.Entity<CancellationPolicy>().ToTable("CancellationPolicies").HasKey(x => x.PolicyId);
        modelBuilder.Entity<Customer>().ToTable("Customers", tb => tb.HasTrigger("trg_Customers")).HasKey(x => x.CustomerId);
        modelBuilder.Entity<MemberCard>().ToTable("MemberCards", tb => tb.HasTrigger("trg_MemberCards")).HasKey(x => x.CardId);
        modelBuilder.Entity<Invoice>().ToTable("Invoices", tb => tb.HasTrigger("trg_Invoices")).HasKey(x => x.InvoiceId);
        modelBuilder.Entity<InvoiceItem>().ToTable("InvoiceItems").HasKey(x => x.ItemId);
        modelBuilder.Entity<Deposit>().ToTable("Deposits").HasKey(x => x.DepositId);
        modelBuilder.Entity<DepositNotification>().ToTable("DepositNotifications").HasKey(x => x.NotifId);
        modelBuilder.Entity<Return>().ToTable("Returns").HasKey(x => x.ReturnId);
        modelBuilder.Entity<ReturnItem>().ToTable("ReturnItems").HasKey(x => x.RetItemId);
        modelBuilder.Entity<AccessLog>().ToTable("AccessLogs").HasKey(x => x.LogId);

        modelBuilder.Entity<UserAccount>()
            .HasOne(x => x.Role)
            .WithMany(x => x.UserAccounts)
            .HasForeignKey(x => x.RoleId);

        modelBuilder.Entity<Product>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId);

        modelBuilder.Entity<Product>()
            .HasOne(x => x.Manufacturer)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.ManufacturerId);

        modelBuilder.Entity<Product>()
            .HasOne(x => x.Supplier)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.SupplierId);

        modelBuilder.Entity<MemberCard>()
            .HasOne(x => x.Customer)
            .WithOne(x => x.MemberCard)
            .HasForeignKey<MemberCard>(x => x.CustomerId);

        modelBuilder.Entity<Invoice>()
            .HasOne(x => x.Cashier)
            .WithMany()
            .HasForeignKey(x => x.CashierId);

        modelBuilder.Entity<Invoice>()
            .HasMany(x => x.Items)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId);
    }
}
