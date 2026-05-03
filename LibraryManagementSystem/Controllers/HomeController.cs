using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly LibraryDbContext _context;

        public HomeController(LibraryDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!_context.Books.Any())
            {
                var book = new Book
                {
                    Title = "Test Book",
                    Author = "Author A",
                    Genre = "Fiction",
                    ISBN = "123456",
                    AvailableCopies = 5
                };

                _context.Books.Add(book);
                _context.SaveChanges();
            }

            if (!_context.BorrowingPolicies.Any())
            {
                var policy = new BorrowingPolicy
                {
                    LoanDurationDays = 7,
                    MaxRenewals = 2,
                    MaxBooksPerMember = 3,
                    FinePerDay = 2
                };

                _context.BorrowingPolicies.Add(policy);
                _context.SaveChanges();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}