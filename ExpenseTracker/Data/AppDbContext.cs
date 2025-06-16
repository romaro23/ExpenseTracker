using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ExpenseEntry> Expenses { get; set; }
        public DbSet<IncomeEntry> Incomes { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(@"Server=ROMARO-PC\SQLEXPRESS;Database=ExpenseDb;Trusted_Connection=True;TrustServerCertificate=True;");

        }
    }
}
