using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JiraApp.Models.Tasks;
using JiraApp.Models.Projects;
using JiraApp.Models.Authentication;
using JiraApp.Models.Database;
using JiraApp.Services.Interfaces;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JiraApp.Controllers
{
    public class TaskController : Controller
    {
        private readonly JiraAppContext _context;
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly IUserService _userService;
        private readonly ILogger<TaskController> _logger;

        public TaskController(JiraAppContext context, ITaskService taskService, IProjectService projectService, IUserService userService, ILogger<TaskController> logger)
        {
            _context = context;
            _taskService = taskService;
            _projectService = projectService;
            _userService = userService;
            _logger = logger;
        }

        // GET: Task/Create
        public async Task<IActionResult> Create(int projectId)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get the project
                var project = await _projectService.GetProjectByIdAsync(projectId);
                if (project == null)
                {
                    return NotFound();
                }

                // Create a new task with the project already set
                var task = new TaskItem
                {
                    ProjectId = projectId,
                    Status = TaskStatusEnum.ToDo,
                    WorkType = WorkType.Task,
                    Priority = Priority.Medium
                };

                // Set reporter to current user if user ID is available
                var userIdString = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    task.ReporterId = userId;
                }

                // Get all users for assignee dropdown
                ViewBag.Users = await _userService.GetAllUsersAsync();
                ViewBag.ProjectName = project.Name;

                return View(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Task Create GET action");
                TempData["ErrorMessage"] = "Error loading create task form. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Task/Create
        [HttpPost]
     
        public async Task<IActionResult> Create(TaskItem task)
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
                    // Set reporter to current user if not set
                    if (task.ReporterId == 0)
                    {
                        var userIdString = HttpContext.Session.GetString("UserId");
                        if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                        {
                            task.ReporterId = userId;
                        }
                    }

                    // Set created date
                    task.CreatedAt = DateTime.Now;

                    await _taskService.CreateTaskAsync(task);

                    return RedirectToAction("Details", "Project", new { id = task.ProjectId });
                }

                // If we got this far, something failed, redisplay form
                ViewBag.Users = await _userService.GetAllUsersAsync();
                ViewBag.Projects = await _projectService.GetAllProjectsAsync();
                var project = await _projectService.GetProjectByIdAsync(task.ProjectId);
                ViewBag.ProjectName = project?.Name ?? "Unknown Project";
                
                return View(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Task Create POST action");
                TempData["ErrorMessage"] = "Error creating task. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Task/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var task = await _taskService.GetTaskByIdAsync(id);
                
                if (task == null)
                {
                    return NotFound();
                }
                
                return View(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Task Details action");
                TempData["ErrorMessage"] = "Error loading task details. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var task = await _taskService.GetTaskByIdAsync(id);
                
                if (task == null)
                {
                    return NotFound();
                }
                
                // Get all users for assignee dropdown
                ViewBag.Users = await _userService.GetAllUsersAsync();
                
                // Get all projects for project dropdown
                ViewBag.Projects = await _projectService.GetAllProjectsAsync();
                
                ViewBag.ProjectName = task.Project?.Name ?? "Unknown Project";
                
                return View(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Task Edit GET action");
                TempData["ErrorMessage"] = "Error loading edit task form. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Task/Edit/5
        [HttpPost]
      
        public async Task<IActionResult> Edit(int id, TaskItem task)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (id != task.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        // If status changed to Done, set completion date
                        if (task.Status == TaskStatusEnum.Done && task.CompletionDate == null)
                        {
                            task.CompletionDate = DateTime.Now;
                        }
                        
                        await _taskService.UpdateTaskAsync(task);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        var taskExists = await _taskService.GetTaskByIdAsync(task.Id) != null;
                        if (!taskExists)
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction("Details", new { id = task.Id });
                }
                
                // If we got this far, something failed, redisplay form
                ViewBag.Users = await _userService.GetAllUsersAsync();
                ViewBag.Projects = await _projectService.GetAllProjectsAsync();
                var project = await _projectService.GetProjectByIdAsync(task.ProjectId);
                ViewBag.ProjectName = project?.Name ?? "Unknown Project";
                
                return View(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Task Edit POST action");
                TempData["ErrorMessage"] = "Error editing task. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }


    }
}
