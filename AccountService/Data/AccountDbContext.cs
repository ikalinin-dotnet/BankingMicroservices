using AccountService.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<Account>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Accounts)
            .HasForeignKey(a => a.CustomerId);
            
        // Add other configurations as needed
    }
}