using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class BorrowingController : Controller
    {
        private readonly LibraryDbContext _context;

        public BorrowingController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: My Borrowings
        public async Task<IActionResult> Index()
        {
            var borrowings = await _context.BorrowingTransactions
                .Where(b => b.MemberId == User.Identity.Name)
                .Include(b => b.Book)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

            return View(borrowings);
        }

        // GET: Borrow (Single Book)
        public async Task<IActionResult> Borrow(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);

            if (book == null)
            {
                TempData["Error"] = "Book not found!";
                return RedirectToAction("Index", "Books");
            }

            if (book.AvailableCopies <= 0)
            {
                // Auto-reserve if unavailable
                var existingReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.MemberId == User.Identity.Name
                                              && r.BookId == bookId
                                              && r.Status == ReservationStatus.Waiting);

                if (existingReservation != null)
                {
                    TempData["Warning"] = $"You already reserved '{book.Title}'!";
                    return RedirectToAction("Index", "Books");
                }

                var reservation = new Reservation
                {
                    MemberId = User.Identity.Name,
                    BookId = bookId,
                    ReservationDate = DateTime.Now,
                    Status = ReservationStatus.Waiting
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["Info"] = $"'{book.Title}' not available. Added to your reservations!";
                return RedirectToAction("Index", "Books");
            }

            var policy = await _context.BorrowingPolicies.FirstOrDefaultAsync();
            if (policy == null)
            {
                TempData["Error"] = "Borrowing policy not configured!";
                return RedirectToAction("Index", "Books");
            }

            var currentBorrowCount = await _context.BorrowingTransactions
                .CountAsync(b => b.MemberId == User.Identity.Name && b.Status == BorrowingStatus.Active);

            if (currentBorrowCount >= policy.MaxBooksPerMember)
            {
                TempData["Error"] = $"Borrowing limit reached! ({policy.MaxBooksPerMember} books max)";
                return RedirectToAction("Index", "Books");
            }

            var transaction = new BorrowingTransaction
            {
                MemberId = User.Identity.Name,
                BookId = bookId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(policy.LoanDurationDays),
                Status = BorrowingStatus.Active,
                RenewalCount = 0
            };

            book.AvailableCopies -= 1;
            _context.BorrowingTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{book.Title}' borrowed successfully! Due date: {transaction.DueDate:dd MMM yyyy}";
            return RedirectToAction("Index");
        }

        // POST: Reserve Multiple Books
        [HttpPost]
        public async Task<IActionResult> ReserveMultiple(int[] selectedBookIds)
        {
            if (selectedBookIds == null || selectedBookIds.Length == 0)
            {
                TempData["Error"] = "Please select at least one book to reserve!";
                return RedirectToAction("Index", "Books");
            }

            int reservedCount = 0;
            int skippedCount = 0;
            var reservedTitles = new List<string>();

            foreach (var bookId in selectedBookIds)
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null) continue;

                // Check if already reserved
                var existingReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.MemberId == User.Identity.Name
                                              && r.BookId == bookId
                                              && r.Status == ReservationStatus.Waiting);

                if (existingReservation != null)
                {
                    skippedCount++;
                    continue;
                }

                var reservation = new Reservation
                {
                    MemberId = User.Identity.Name,
                    BookId = bookId,
                    ReservationDate = DateTime.Now,
                    Status = ReservationStatus.Waiting
                };

                _context.Reservations.Add(reservation);
                reservedCount++;
                reservedTitles.Add(book.Title);
            }

            await _context.SaveChangesAsync();

            if (reservedCount > 0)
            {
                TempData["Success"] = $"✅ Successfully reserved {reservedCount} book(s)!" +
                    (skippedCount > 0 ? $" ({skippedCount} already reserved)" : "");
            }
            else
            {
                TempData["Warning"] = "No new reservations made. Books may already be reserved.";
            }

            return RedirectToAction("Reservations");
        }

        // GET: Reserve (Single Book)
        public async Task<IActionResult> Reserve(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["Error"] = "Book not found!";
                return RedirectToAction("Index", "Books");
            }

            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.MemberId == User.Identity.Name
                                          && r.BookId == bookId
                                          && r.Status == ReservationStatus.Waiting);

            if (existingReservation != null)
            {
                TempData["Warning"] = $"You already reserved '{book.Title}'!";
                return RedirectToAction("Reservations");
            }

            var reservation = new Reservation
            {
                MemberId = User.Identity.Name,
                BookId = bookId,
                ReservationDate = DateTime.Now,
                Status = ReservationStatus.Waiting
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{book.Title}' reserved successfully!";
            return RedirectToAction("Reservations");
        }

        // GET: Cancel Reservation
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found!";
                return RedirectToAction("Reservations");
            }

            if (reservation.MemberId != User.Identity.Name)
            {
                TempData["Error"] = "You cannot cancel someone else's reservation!";
                return RedirectToAction("Reservations");
            }

            reservation.Status = ReservationStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Reservation for '{reservation.Book?.Title}' cancelled!";
            return RedirectToAction("Reservations");
        }

        // GET: Return Book
        public async Task<IActionResult> Return(int id)
        {
            var transaction = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found!";
                return RedirectToAction("Index");
            }

            if (transaction.MemberId != User.Identity.Name)
            {
                TempData["Error"] = "You cannot return another member's book!";
                return RedirectToAction("Index");
            }

            transaction.ReturnDate = DateTime.Now;
            transaction.Status = BorrowingStatus.Returned;

            if (transaction.Book != null)
            {
                transaction.Book.AvailableCopies += 1;
            }

            var policy = await _context.BorrowingPolicies.FirstOrDefaultAsync();
            if (transaction.ReturnDate > transaction.DueDate && policy != null)
            {
                var daysLate = (transaction.ReturnDate.Value - transaction.DueDate).Days;
                var fine = new Fine
                {
                    MemberId = transaction.MemberId,
                    TransactionId = transaction.Id,
                    Amount = daysLate * policy.FinePerDay,
                    IssuedDate = DateTime.Now,
                    Status = FineStatus.Unpaid
                };
                _context.Fines.Add(fine);
                TempData["Warning"] = $"Returned late! Fine: ₹{fine.Amount} ({daysLate} days)";
            }
            else
            {
                TempData["Success"] = $"'{transaction.Book?.Title}' returned successfully!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Renew Book
        public async Task<IActionResult> Renew(int id)
        {
            var transaction = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found!";
                return RedirectToAction("Index");
            }

            if (transaction.MemberId != User.Identity.Name)
            {
                TempData["Error"] = "You cannot renew another member's book!";
                return RedirectToAction("Index");
            }

            var policy = await _context.BorrowingPolicies.FirstOrDefaultAsync();
            if (policy == null)
            {
                TempData["Error"] = "Policy not found!";
                return RedirectToAction("Index");
            }

            if (transaction.RenewalCount >= policy.MaxRenewals)
            {
                TempData["Error"] = $"Renewal limit reached! ({policy.MaxRenewals} max)";
                return RedirectToAction("Index");
            }

            transaction.DueDate = transaction.DueDate.AddDays(policy.LoanDurationDays);
            transaction.RenewalCount += 1;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{transaction.Book?.Title}' renewed! New due: {transaction.DueDate:dd MMM yyyy}";
            return RedirectToAction("Index");
        }

        // GET: Manage (Librarian)
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Manage()
        {
            var allTransactions = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .OrderByDescending(t => t.BorrowDate)
                .ToListAsync();
            return View(allTransactions);
        }

        // GET: Policy
        public async Task<IActionResult> Policy()
        {
            var policy = await _context.BorrowingPolicies.FirstOrDefaultAsync();

            if (policy == null)
            {
                policy = new BorrowingPolicy
                {
                    LoanDurationDays = 7,
                    MaxRenewals = 2,
                    MaxBooksPerMember = 3,
                    FinePerDay = 2
                };
                _context.BorrowingPolicies.Add(policy);
                await _context.SaveChangesAsync();
            }

            return View(policy);
        }

        [HttpPost]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Policy(BorrowingPolicy policy)
        {
            if (ModelState.IsValid)
            {
                _context.BorrowingPolicies.Update(policy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Policy updated successfully!";
                return RedirectToAction("Policy");
            }
            return View(policy);
        }

        // GET: My Reservations
        public async Task<IActionResult> Reservations()
        {
            var reservations = await _context.Reservations
                .Where(r => r.MemberId == User.Identity.Name)
                .Include(r => r.Book)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(reservations);
        }
    }
}