using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class LibrarianController : Controller
    {
        private readonly LibraryDbContext _context;

        public LibrarianController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Librarian Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Statistics
            ViewBag.TotalBooks = await _context.Books.CountAsync();
            ViewBag.TotalMembers = await _context.Users.CountAsync(u => u.Role == "Member");
            ViewBag.TotalLibrarians = await _context.Users.CountAsync(u => u.Role == "Librarian");
            ViewBag.ActiveBorrowings = await _context.BorrowingTransactions
                .CountAsync(b => b.Status == BorrowingStatus.Active);
            ViewBag.OverdueBooks = await _context.BorrowingTransactions
                .CountAsync(b => b.Status == BorrowingStatus.Overdue);
            ViewBag.PendingReservations = await _context.Reservations
                .CountAsync(r => r.Status == ReservationStatus.Waiting);
            ViewBag.TotalFines = await _context.Fines
                .Where(f => f.Status == FineStatus.Unpaid)
                .SumAsync(f => (decimal?)f.Amount) ?? 0;

            // Calculate available copies
            ViewBag.AvailableCopies = await _context.Books.SumAsync(b => b.AvailableCopies);

            // Recent Activities - Last 5 borrowings
            ViewBag.RecentBorrowings = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .OrderByDescending(b => b.BorrowDate)
                .Take(5)
                .ToListAsync();

            // Recent Members
            ViewBag.RecentMembers = await _context.Users
                .Where(u => u.Role == "Member")
                .OrderByDescending(u => u.RegisteredDate)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // GET: Manage Members
        public async Task<IActionResult> Members()
        {
            var members = await _context.Users
                .Where(u => u.Role == "Member")
                .OrderByDescending(u => u.RegisteredDate)
                .ToListAsync();

            return View(members);
        }

        // POST: Toggle Member Status
        [HttpPost]
        public async Task<IActionResult> ToggleMemberStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null && user.Role == "Member")
            {
                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Member {(user.IsActive ? "activated" : "deactivated")} successfully!";
            }
            return RedirectToAction("Members");
        }

        // GET: Delete Member Confirmation
        public async Task<IActionResult> DeleteMember(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Member")
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Delete Member
        [HttpPost, ActionName("DeleteMember")]
        public async Task<IActionResult> DeleteMemberConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null && user.Role == "Member")
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member deleted successfully!";
            }
            return RedirectToAction("Members");
        }
    }
}