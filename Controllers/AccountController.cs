using Microsoft.AspNetCore.Mvc;
using JiraApp.Models.Authentication;
using JiraApp.Models.Database;
using JiraApp.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using JiraApp.Models.Tasks;

namespace JiraApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly JiraAppContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(JiraAppContext context, IUserService userService, ILogger<AccountController> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            try
            {
                // If already logged in, redirect to board
                if (HttpContext.Session.GetString("UserId") != null)
                {
                    return RedirectToAction("Index", "Board");
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login GET action");
                TempData["ErrorMessage"] = "Database connection error. Please check your connection string and ensure SQL Server is running.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Account/Login
        [HttpPost]
        
        public IActionResult Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Username and password are required");
                    return View();
                }

                var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
                
                if (user != null)
                {
                    // Store user info in session
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    
                    return RedirectToAction("Index", "Board");
                }
                
                ModelState.AddModelError("", "Invalid username or password");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError("", "An error occurred during login. Please check your database connection.");
                return View();
            }
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            try
            {
                return View(new User { Role = "User" });  // Pre-set the Role to "User"
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Register GET action");
                TempData["ErrorMessage"] = "Database connection error. Please check your connection string and ensure SQL Server is running.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Account/Register
        [HttpPost]
       
        public IActionResult Register(User user)
        {
            try
            {
                // Set default role to "User" before validation
                if (string.IsNullOrEmpty(user.Role))
                {
                    user.Role = "User";
                }

                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                {
                    ModelState.AddModelError("", "Username and password are required");
                    return View(user);
                }

                // Check if username already exists - use a safer approach
                bool usernameExists = false;
                try
                {
                    usernameExists = _context.Users.Any(u => u.Username == user.Username);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking if username exists");
                    TempData["ErrorMessage"] = "Database connection error. Please try again later.";
                    return RedirectToAction("Error", "Home");
                }

                if (usernameExists)
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(user);
                }
                
                // Initialize collections to avoid null reference exceptions
                user.AssignedTasks = new System.Collections.Generic.List<TaskItem>();
                user.ReportedTasks = new System.Collections.Generic.List<TaskItem>();
                
                _context.Users.Add(user);
                _context.SaveChanges();
                
                // Auto-login after registration
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role);
                
                return RedirectToAction("Index", "Board");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error during registration");
                TempData["ErrorMessage"] = "A database error occurred. Please check your connection string and ensure SQL Server is running.";
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred during registration. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            try
            {
                // Clear session
                HttpContext.Session.Clear();
                
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Login");
            }
        }

        // GET: Account/CreateAdmin
        public IActionResult CreateAdmin()
        {
            try
            {
                // Only allow access if no users exist in the database
                if (_context.Users.Any())
                {
                    return RedirectToAction("Login");
                }
                
                return View(new User { Role = "Admin" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAdmin GET action");
                TempData["ErrorMessage"] = "Database connection error. Please check your connection string and ensure SQL Server is running.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Account/CreateAdmin
        [HttpPost]
       
        public IActionResult CreateAdmin(User user)
        {
            try
            {
                // Only allow creation if no users exist in the database
                if (_context.Users.Any())
                {
                    return RedirectToAction("Login");
                }
                
                // Ensure the role is Admin
                user.Role = "Admin";

                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                {
                    ModelState.AddModelError("", "Username and password are required");
                    return View(user);
                }
                
                // Initialize collections to avoid null reference exceptions
                user.AssignedTasks = new System.Collections.Generic.List<TaskItem>();
                user.ReportedTasks = new System.Collections.Generic.List<TaskItem>();
                
                _context.Users.Add(user);
                _context.SaveChanges();
                
                _logger.LogInformation("Created admin user: {Username}", user.Username);
                
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin creation: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred during admin creation. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
