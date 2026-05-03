using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly LibraryDbContext _context;

        public MemberController(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var memberId = User.FindFirst("MemberId")?.Value ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(memberId))
            {
                return RedirectToAction("Login", "Account");
            }

            var allBorrowings = await _context.BorrowingTransactions
                .Where(b => b.MemberId == memberId)
                .Include(b => b.Book)
                .ToListAsync();

            ViewBag.TotalBorrowings = allBorrowings.Count;
            ViewBag.ActiveBorrowings = allBorrowings.Count(b => b.Status == BorrowingStatus.Active);
            ViewBag.ReturnedBooks = allBorrowings.Count(b => b.Status == BorrowingStatus.Returned);
            ViewBag.OverdueBooks = allBorrowings.Count(b => b.Status == BorrowingStatus.Overdue);

            var reservations = await _context.Reservations
                .Where(r => r.MemberId == memberId && r.Status == ReservationStatus.Waiting)
                .Include(r => r.Book)
                .ToListAsync();

            ViewBag.ActiveReservations = reservations.Count;

            var fines = await _context.Fines
                .Where(f => f.MemberId == memberId && f.Status == FineStatus.Unpaid)
                .ToListAsync();

            ViewBag.TotalFines = fines.Sum(f => f.Amount);
            ViewBag.UnpaidFinesCount = fines.Count;

            ViewBag.RecentBorrowings = allBorrowings
                .OrderByDescending(b => b.BorrowDate)
                .Take(5)
                .ToList();

            ViewBag.Reservations = reservations.Take(5).ToList();

            // CRITICAL FIX: First fetch, then randomize in memory
            var availableBooks = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .ToListAsync();

            ViewBag.RecommendedBooks = availableBooks
                .OrderBy(b => Guid.NewGuid())
                .Take(4)
                .ToList();

            if (int.TryParse(memberId, out int userIdInt))
            {
                ViewBag.Member = await _context.Users.FindAsync(userIdInt);
            }

            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var memberId = User.FindFirst("UserId")?.Value;
            if (int.TryParse(memberId, out int id))
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    return View(user);
                }
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            var memberId = User.FindFirst("UserId")?.Value;
            if (int.TryParse(memberId, out int id))
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    user.FullName = model.FullName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");
                }
            }
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> History()
        {
            var memberId = User.FindFirst("MemberId")?.Value ?? User.FindFirst("UserId")?.Value;

            var history = await _context.BorrowingTransactions
                .Where(b => b.MemberId == memberId)
                .Include(b => b.Book)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

            return View(history);
        }
    }
}