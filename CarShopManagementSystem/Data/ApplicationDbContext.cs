using CarShopManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarShopManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }

        // ✅ الجديد
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<SubBrand> SubBrands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // علاقة Manufacturer -> SubBrands
            modelBuilder.Entity<Manufacturer>()
                .HasMany(m => m.SubBrands)
                .WithOne(sb => sb.Manufacturer)
                .HasForeignKey(sb => sb.ManufacturerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@carshop.com",
                    Password = "Admin123!", // In a real application, this should be hashed
                    Role = "Admin",
                    IsAdmin = true,
                    RegisterDate = DateTime.Now
                }
            );

            // Seed credit cards (8 valid, 2 invalid)
            modelBuilder.Entity<CreditCard>().HasData(
                new CreditCard { Id = 1, CardNumber = "4111111111111111", HolderName = "John Doe", ExpiryMonth = 12, ExpiryYear = DateTime.Now.Year + 2, CVV = "123", Balance = 50000, IsValid = true },
                new CreditCard { Id = 2, CardNumber = "5500000000000004", HolderName = "Jane Smith", ExpiryMonth = 11, ExpiryYear = DateTime.Now.Year + 3, CVV = "456", Balance = 45000, IsValid = true },
                new CreditCard { Id = 3, CardNumber = "340000000000009", HolderName = "Ali Ahmad", ExpiryMonth = 10, ExpiryYear = DateTime.Now.Year + 4, CVV = "789", Balance = 60000, IsValid = true },
                new CreditCard { Id = 4, CardNumber = "30000000000004", HolderName = "Sara Ibrahim", ExpiryMonth = 9, ExpiryYear = DateTime.Now.Year + 1, CVV = "012", Balance = 10000, IsValid = true },
                new CreditCard { Id = 5, CardNumber = "6011000000000004", HolderName = "Mohamed Ali", ExpiryMonth = 8, ExpiryYear = DateTime.Now.Year + 5, CVV = "345", Balance = 8000, IsValid = true },
                new CreditCard { Id = 6, CardNumber = "3530111333300000", HolderName = "Lina Hassan", ExpiryMonth = 7, ExpiryYear = DateTime.Now.Year + 2, CVV = "678", Balance = 12000, IsValid = true },
                new CreditCard { Id = 7, CardNumber = "6331101999990016", HolderName = "Omar Saleh", ExpiryMonth = 6, ExpiryYear = DateTime.Now.Year + 3, CVV = "901", Balance = 30000, IsValid = true },
                new CreditCard { Id = 8, CardNumber = "6759649826438453", HolderName = "Rania Khaled", ExpiryMonth = 5, ExpiryYear = DateTime.Now.Year + 4, CVV = "234", Balance = 15000, IsValid = true },
                new CreditCard { Id = 9, CardNumber = "4111111111111129", HolderName = "Invalid Card", ExpiryMonth = 4, ExpiryYear = DateTime.Now.Year + 2, CVV = "567", Balance = 20000, IsValid = false },
                new CreditCard { Id = 10, CardNumber = "5500000000000051", HolderName = "No Funds", ExpiryMonth = 3, ExpiryYear = DateTime.Now.Year + 2, CVV = "890", Balance = 0, IsValid = true }
            );
        }
    }
}
