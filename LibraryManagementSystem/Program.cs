using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure SQLite Database with LibraryDbContext
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Cookie Authentication (for "LibraryCookie" scheme)
builder.Services.AddAuthentication("LibraryCookie")
    .AddCookie("LibraryCookie", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContext accessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Auto-create database on startup (Azure Fix)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var dbPath = db.Database.GetDbConnection().DataSource;
        var dbDirectory = Path.GetDirectoryName(dbPath);
        
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }
        
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// IMPORTANT: Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
