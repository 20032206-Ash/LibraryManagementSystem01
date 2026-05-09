using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryDbContext _context;
     // heelooooooo

        public AccountController(LibraryDbContext context)
        {
            _context = context;
        }

        // Login Selection Page
        public IActionResult Login()
        {
            return View();
        }

        // ==================== LIBRARIAN LOGIN ====================
        [HttpGet]
        public IActionResult LibrarianLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LibrarianLogin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email && u.Password == password && u.Role == "Librarian" && u.IsActive);

            if (user == null)
            {
                ViewBag.Error = "Invalid Librarian credentials.";
                return View();
            }

            await SignInUser(user);
            return RedirectToAction("Dashboard", "Librarian");
        }

        // ==================== MEMBER LOGIN ====================
        [HttpGet]
        public IActionResult MemberLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MemberLogin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email && u.Password == password && u.Role == "Member" && u.IsActive);

            if (user == null)
            {
                ViewBag.Error = "Invalid Member credentials.";
                return View();
            }

            await SignInUser(user);
            return RedirectToAction("Dashboard", "Member");
        }

        // ==================== REGISTER (MEMBER) ====================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                ViewBag.Error = "Email already registered. Please login.";
                return View(user);
            }

            user.Role = "Member";
            user.RegisteredDate = DateTime.Now;
            user.IsActive = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registration successful! Please login.";
            return RedirectToAction("MemberLogin");
        }

        // ==================== LOGOUT ====================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("LibraryCookie");
            return RedirectToAction("Index", "Home");
        }

        // ==================== HELPER METHOD ====================
        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString()),
                new Claim("MemberId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "LibraryCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("LibraryCookie", principal);
        }
    }
}