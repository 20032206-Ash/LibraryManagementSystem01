using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Books Index (Browse all books)
        public async Task<IActionResult> Index(string searchTerm, string genre)
        {
            var books = _context.Books.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchTerm) ||
                    b.Author.Contains(searchTerm) ||
                    b.ISBN.Contains(searchTerm));
            }

            // Genre filter
            if (!string.IsNullOrEmpty(genre) && genre != "All")
            {
                books = books.Where(b => b.Genre == genre);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedGenre = genre;
            ViewBag.Genres = await _context.Books
                .Where(b => !string.IsNullOrEmpty(b.Genre))
                .Select(b => b.Genre)
                .Distinct()
                .ToListAsync();

            return View(await books.ToListAsync());
        }

        // GET: Book Details
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // ==================== LIBRARIAN ACTIONS ====================

        // GET: Create Book (Librarian only)
        [Authorize(Roles = "Librarian")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Book
        [HttpPost]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Book '{book.Title}' added successfully!";
                return RedirectToAction("Index");
            }
            return View(book);
        }

        // GET: Edit Book
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Edit Book
        [HttpPost]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Book '{book.Title}' updated successfully!";
                return RedirectToAction("Index");
            }
            return View(book);
        }

        // GET: Delete Book
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Delete Book
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Book '{book.Title}' deleted successfully!";
            }
            return RedirectToAction("Index");
        }
    }
}