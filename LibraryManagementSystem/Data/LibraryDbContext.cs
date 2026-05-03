using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }  // ✅ NEW - Users table
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowingTransaction> BorrowingTransactions { get; set; }
        public DbSet<BorrowingPolicy> BorrowingPolicies { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Fine> Fines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed a default Librarian account
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Admin Librarian",
                    Email = "admin@library.com",
                    Password = "admin123",
                    Role = "Librarian",
                    PhoneNumber = "1234567890",
                    Address = "Library Main Office",
                    RegisteredDate = new DateTime(2025, 1, 1),
                    IsActive = true
                },
                new User
                {
                    Id = 2,
                    FullName = "Test Member",
                    Email = "member@library.com",
                    Password = "member123",
                    Role = "Member",
                    PhoneNumber = "9876543210",
                    Address = "123 Member Street",
                    RegisteredDate = new DateTime(2025, 1, 1),
                    IsActive = true
                }
            );
        }
    }
}