using Microsoft.AspNetCore.Mvc;
using JiraApp.Models.Projects;
using JiraApp.Models.Tasks;
using JiraApp.Models.Authentication;
using JiraApp.Models.Database;
using JiraApp.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace JiraApp.Controllers
{
    public class ProjectController : Controller
    {
        private readonly JiraAppContext _context;
        private readonly IProjectService _projectService;
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(JiraAppContext context, IProjectService projectService, ITaskService taskService, IUserService userService, ILogger<ProjectController> logger)
        {
            _context = context;
            _projectService = projectService;
            _taskService = taskService;
            _userService = userService;
            _logger = logger;
        }

        // GET: Project
        public IActionResult Index()
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get all projects
                var projects = _context.Projects.ToList();
                return View(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Project Index action");
                TempData["ErrorMessage"] = "Error loading projects. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Project/Create
        public IActionResult Create()
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // All users can create projects now
                return View(new Project());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Project Create GET action");
                TempData["ErrorMessage"] = "Error loading create project form. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Project/Create
        [HttpPost]
      
        public IActionResult Create(Project project)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (ModelState.IsValid)
                {
                    // Set created date to now
                    project.CreatedDate = DateTime.Now;
                    
                    // Add project to database
                    _context.Projects.Add(project);
                    _context.SaveChanges();
                    
                    return RedirectToAction(nameof(Index));
                }
                
                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Project Create POST action");
                ModelState.AddModelError("", "Error creating project. Please try again.");
                return View(project);
            }
        }

        // GET: Project/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get project with tasks
                var project = _context.Projects
                    .Include(p => p.Tasks)
                    .FirstOrDefault(p => p.Id == id);
                
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction("Error", "Home");
                }
                
                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Project Details action");
                TempData["ErrorMessage"] = "Error loading project details. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Project/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get project
                var project = _context.Projects.Find(id);
                
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction("Error", "Home");
                }
                
                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Project Edit GET action");
                TempData["ErrorMessage"] = "Error loading edit project form. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Project/Edit/5
        [HttpPost]
      
        public IActionResult Edit(int id, Project project)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (id != project.Id)
                {
                    TempData["ErrorMessage"] = "Invalid project ID.";
                    return RedirectToAction("Error", "Home");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(project);
                        _context.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProjectExists(project.Id))
                        {
                            TempData["ErrorMessage"] = "Project not found.";
                            return RedirectToAction("Error", "Home");
                        }
                        else
                        {
                            throw;
                        }
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                
                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Project Edit POST action");
                ModelState.AddModelError("", "Error updating project. Please try again.");
                return View(project);
            }
        }

        // Helper method to check if project exists
        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
