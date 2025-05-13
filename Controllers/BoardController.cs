using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JiraApp.Models.Tasks;
using JiraApp.Models.Projects;
using JiraApp.Models.Authentication;
using JiraApp.Models.Database;
using JiraApp.ViewModels.Board;
using JiraApp.Services.Interfaces;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace JiraApp.Controllers
{
    public class BoardController : Controller
    {
        private readonly JiraAppContext _context;
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly IUserService _userService;
        private readonly ILogger<BoardController> _logger;

        public BoardController(JiraAppContext context, ITaskService taskService, IProjectService projectService, IUserService userService, ILogger<BoardController> logger)
        {
            _context = context;
            _taskService = taskService;
            _projectService = projectService;
            _userService = userService;
            _logger = logger;
        }

        // GET: Board
        public async Task<IActionResult> Index(int? projectId = null)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get all projects for the sidebar
                var allProjects = await _context.Projects
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                
                // Pass projects to the view for the sidebar
                ViewBag.Projects = allProjects;
                
                // If a project is selected, store its ID in ViewBag
                ViewBag.SelectedProjectId = projectId;

                // Create a simple view model with default empty collections
                var viewModel = new BoardViewModel
                {
                    CurrentUserRole = HttpContext.Session.GetString("UserRole") ?? "User",
                    Tasks = new List<TaskItem>(),
                    Users = new List<User>(),
                    Projects = new List<Project>()
                };

                // Safely load users
                try
                {
                    viewModel.Users = (await _userService.GetAllUsersAsync()).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load users, continuing with empty collection");
                }

                // Safely load projects
                try
                {
                    viewModel.Projects = (await _projectService.GetAllProjectsAsync()).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load projects, continuing with empty collection");
                }

                // Safely load tasks with related entities
                try
                {
                    viewModel.Tasks = (await _taskService.GetAllTasksAsync()).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load tasks, continuing with empty collection");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Board Index action: {Message}", ex.Message);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Error loading board. Please try again." });
                }
                TempData["ErrorMessage"] = "Error loading board. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Board/GetTasksByUser/5
        public IActionResult GetTasksByUser(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Get user
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Get tasks assigned to user
                var assignedTasks = _context.Tasks
                    .Include(t => t.Assignee)
                    .Include(t => t.Reporter)
                    .Include(t => t.Project)
                    .Where(t => t.AssigneeId == id)
                    .ToList();

                // Get tasks reported by user
                var reportedTasks = _context.Tasks
                    .Include(t => t.Assignee)
                    .Include(t => t.Reporter)
                    .Include(t => t.Project)
                    .Where(t => t.ReporterId == id)
                    .ToList();

                // Create view model
                var viewModel = new UserTasksViewModel
                {
                    User = user,
                    AssignedTasks = assignedTasks,
                    ReportedTasks = reportedTasks
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTasksByUser action");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Error loading user tasks. Please try again." });
                }
                TempData["ErrorMessage"] = "Error loading user tasks. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Board/CreateTask
        public IActionResult CreateTask()
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Please log in to create a task." });
                    }
                    return RedirectToAction("Login", "Account");
                }

                // Get all users for assignee dropdown
                var users = new List<User>();
                try
                {
                    users = _context.Users.ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load users for task creation");
                }
                ViewBag.Users = users;
                
                // Get all projects for project dropdown
                var projects = new List<Project>();
                try
                {
                    projects = _context.Projects.ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load projects for task creation");
                }
                ViewBag.Projects = projects;

                // Set reporter to current user
                var task = new TaskItem();
                var userIdString = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    task.ReporterId = userId;
                    ViewBag.CurrentUserId = userId;
                }

                // Set default values
                task.Status = TaskStatusEnum.ToDo;
                task.WorkType = WorkType.Task;
                task.Priority = Priority.Medium;
                task.CreatedAt = DateTime.Now;

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("Modals/_CreateTask", task);
                }
                return View(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateTask GET action");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Error loading create task form." });
                }
                
                TempData["ErrorMessage"] = "Error loading create task form. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Board/SaveTask
        [HttpPost]
        public IActionResult SaveTask(string Title, string Description, int ProjectId, int WorkType, int Priority, int AssigneeId, int Status)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return Json(new { success = false, message = "Please log in to create a task." });
                }

                // Log the received task data
                _logger.LogInformation("Received task data: Title={Title}, ProjectId={ProjectId}, WorkType={WorkType}, Priority={Priority}, AssigneeId={AssigneeId}, Status={Status}",
                    Title, ProjectId, WorkType, Priority, AssigneeId, Status);

                // Verify project exists
                var project = _context.Projects.Find(ProjectId);
                if (project == null)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found", ProjectId);
                    return Json(new { success = false, message = "The selected project does not exist." });
                }

                // Verify assignee exists
                var assignee = _context.Users.Find(AssigneeId);
                if (assignee == null)
                {
                    _logger.LogWarning("User with ID {AssigneeId} not found", AssigneeId);
                    return Json(new { success = false, message = "The selected assignee does not exist." });
                }

                // Get current user ID for reporter
                int reporterId;
                var userIdString = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out reporterId))
                {
                    // Verify reporter exists
                    var reporter = _context.Users.Find(reporterId);
                    if (reporter == null)
                    {
                        _logger.LogWarning("Reporter with ID {ReporterId} not found", reporterId);
                        return Json(new { success = false, message = "The current user (reporter) does not exist in the database." });
                    }

                    // Create a new task with the provided data
                    var task = new TaskItem
                    {
                        Title = Title ?? string.Empty,
                        Description = Description ?? string.Empty,
                        ProjectId = ProjectId,
                        WorkType = (WorkType)WorkType,
                        Priority = (Priority)Priority,
                        Status = (TaskStatusEnum)Status,
                        AssigneeId = AssigneeId,
                        ReporterId = reporterId,
                        CreatedAt = DateTime.Now
                    };

                    _logger.LogInformation("Task object created: {@Task}", task);

                    // Add task to database
                    _context.Tasks.Add(task);
                    
                    // Save changes and log any exceptions
                    try
                    {
                        _context.SaveChanges();
                        _logger.LogInformation("Task saved to database successfully: ID={Id}, Title={Title}", 
                            task.Id, task.Title);
                        return Json(new { success = true, message = "Task created successfully.", taskId = task.Id });
                    }
                    catch (DbUpdateException dbEx)
                    {
                        _logger.LogError(dbEx, "Database error while saving task: {Message}", dbEx.Message);
                        if (dbEx.InnerException != null)
                        {
                            _logger.LogError(dbEx.InnerException, "Inner exception: {Message}", dbEx.InnerException.Message);
                        }
                        return Json(new { success = false, message = "Database error while saving task: " + dbEx.Message });
                    }
                }
                else
                {
                    _logger.LogWarning("Could not determine reporter ID from session: {UserIdString}", userIdString);
                    return Json(new { success = false, message = "Could not determine the current user for reporter." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveTask action: {Message}", ex.Message);
                
                // Return more detailed error information
                string errorDetails = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "An error occurred while saving the task: " + errorDetails });
            }
        }

        // POST: Board/CreateTask
        [HttpPost]
     
        public IActionResult CreateTask(TaskItem task)
        {
            try
            {
                _logger.LogInformation("CreateTask POST action called");
                
                // Check if user is logged in
                var userIdString = HttpContext.Session.GetString("UserId");
                if (userIdString == null)
                {
                    _logger.LogWarning("User not logged in when attempting to create task");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "You must be logged in to create a task." });
                    }
                    
                    return RedirectToAction("Login", "Account");
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Model state is valid");
                    
                    // Set reporter to current user
                    int reporterId;
                    if (!int.TryParse(userIdString, out reporterId))
                    {
                        _logger.LogWarning($"Could not parse user ID from session: {userIdString}");
                        
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = "Invalid user session. Please log in again." });
                        }
                        
                        ViewBag.Users = _context.Users.ToList();
                        return View(task);
                    }
                    
                    task.ReporterId = reporterId;
                    
                    // Set default status if not provided
                    if (task.Status == 0) // Default enum value is 0
                    {
                        task.Status = TaskStatusEnum.ToDo;
                    }
                    
                    // Set creation date
                    task.CreatedAt = DateTime.Now;
                    
                    // Don't try to include navigation properties when adding a new entity
                    // EF Core will handle the relationships based on the foreign keys
                    task.Assignee = null;
                    task.Reporter = null;
                    task.Project = null;
                    
                    _logger.LogInformation("About to add task to database");
                    _context.Tasks.Add(task);
                    
                    try
                    {
                        int result = _context.SaveChanges();
                        _logger.LogInformation($"SaveChanges result: {result} rows affected");
                        _logger.LogInformation($"Task created successfully with ID: {task.Id}");
                        
                        // Verify task was added by querying it back
                        var savedTask = _context.Tasks.Find(task.Id);
                        if (savedTask != null)
                        {
                            _logger.LogInformation($"Verified task in database: ID={savedTask.Id}, Title={savedTask.Title}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not verify task with ID {task.Id} in database after save");
                        }
                        
                        // If it's an AJAX request, return JSON result
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            _logger.LogInformation("Returning JSON success response for AJAX request");
                            return Json(new { success = true, taskId = task.Id, message = "Task created successfully!" });
                        }
                        
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving task to database");
                        
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = "Error saving task to database." });
                        }
                        
                        TempData["ErrorMessage"] = "Error saving task to database.";
                        ViewBag.Users = _context.Users.ToList();
                        return RedirectToAction("Error", "Home");
                    }
                }
                else
                {
                    _logger.LogWarning("Model state is invalid");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning($"Model error: {error.ErrorMessage}");
                    }
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Please fill out all required fields correctly." });
                    }
                    
                    ViewBag.Users = _context.Users.ToList();
                    return View(task);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in CreateTask POST action");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while creating the task." });
                }
                
                TempData["ErrorMessage"] = "An error occurred while creating the task. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Board/CreateProject
        public IActionResult CreateProject()
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Please log in to create a project." });
                    }
                    return RedirectToAction("Login", "Account");
                }

                // Create a new project with default values
                var project = new Project
                {
                    Status = ProjectStatus.Active,
                    CreatedDate = DateTime.Now
                };

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("Modals/_CreateProject", project);
                }
                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateProject GET action");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Error loading create project form." });
                }
                
                TempData["ErrorMessage"] = "Error loading create project form. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Board/SaveProject
        [HttpPost]
        public IActionResult SaveProject(string Name, string Description, int Status)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return Json(new { success = false, message = "Please log in to create a project." });
                }

                // Log the received project data
                _logger.LogInformation("Received project data: Name={Name}, Description={Description}, Status={Status}",
                    Name, Description, Status);

                // Create a new project with the provided data
                var project = new Project
                {
                    Name = Name ?? string.Empty,
                    Description = Description ?? string.Empty,
                    Status = (ProjectStatus)Status,
                    CreatedDate = DateTime.Now,
                    Tasks = new List<TaskItem>()
                };

                // Add project to database
                _context.Projects.Add(project);
                _context.SaveChanges();

                _logger.LogInformation("Project created successfully: ID={Id}, Name={Name}", 
                    project.Id, project.Name);

                return Json(new { success = true, message = "Project created successfully.", projectId = project.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveProject action: {Message}", ex.Message);
                
                // Return more detailed error information
                string errorDetails = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "An error occurred while saving the project: " + errorDetails });
            }
        }

        // POST: Board/UpdateTaskStatus
        [HttpPost]
        public IActionResult UpdateTaskStatus(int id, string status)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return Json(new { success = false, message = "You must be logged in to update a task." });
                }

                // Find the task
                var task = _context.Tasks.Find(id);
                if (task == null)
                {
                    return Json(new { success = false, message = "Task not found." });
                }

                // Convert string status to enum
                TaskStatusEnum newStatus;
                switch (status)
                {
                    case "ToDo":
                        newStatus = TaskStatusEnum.ToDo;
                        break;
                    case "InProgress":
                        newStatus = TaskStatusEnum.InProgress;
                        break;
                    case "Done":
                        newStatus = TaskStatusEnum.Done;
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid status value." });
                }

                // Update status
                task.Status = newStatus;
                
                // If status is "Done", set completion date
                if (newStatus == TaskStatusEnum.Done && task.CompletionDate == null)
                {
                    task.CompletionDate = DateTime.Now;
                }
                
                _context.Update(task);
                _context.SaveChanges();
                
                return Json(new { success = true, message = "Task status updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateTaskStatus action");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while updating the task status." });
                }
                TempData["ErrorMessage"] = "An error occurred while updating the task status.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Board/ChangeTaskStatus
        [HttpPost]
        public IActionResult ChangeTaskStatus(int taskId, int newStatus)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return Json(new { success = false, message = "You must be logged in to update a task." });
                }

                _logger.LogInformation("Changing task status: TaskId={TaskId}, NewStatus={NewStatus}", taskId, newStatus);

                // Find the task
                var task = _context.Tasks.Find(taskId);
                if (task == null)
                {
                    _logger.LogWarning("Task not found: TaskId={TaskId}", taskId);
                    return Json(new { success = false, message = "Task not found." });
                }

                // Validate the new status value
                if (!Enum.IsDefined(typeof(TaskStatusEnum), newStatus))
                {
                    _logger.LogWarning("Invalid status value: {NewStatus}", newStatus);
                    return Json(new { success = false, message = "Invalid status value." });
                }

                // Update status
                TaskStatusEnum statusEnum = (TaskStatusEnum)newStatus;
                task.Status = statusEnum;
                
                // If status is "Done", set completion date
                if (statusEnum == TaskStatusEnum.Done && task.CompletionDate == null)
                {
                    task.CompletionDate = DateTime.Now;
                }
                else if (statusEnum != TaskStatusEnum.Done)
                {
                    // Reset completion date if task is moved back from Done
                    task.CompletionDate = null;
                }
                
                _context.Update(task);
                _context.SaveChanges();
                
                _logger.LogInformation("Task status updated successfully: TaskId={TaskId}, NewStatus={NewStatus}", taskId, statusEnum);
                return Json(new { success = true, message = "Task status updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChangeTaskStatus action: {Message}", ex.Message);
                return Json(new { success = false, message = "An error occurred while updating the task status." });
            }
        }

        // GET: Board/GetTaskDetails/5
        public IActionResult GetTaskDetails(int id)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Please log in to view task details." });
                    }
                    return RedirectToAction("Login", "Account");
                }

                // Find the task with related entities
                var task = _context.Tasks
                    .Include(t => t.Assignee)
                    .Include(t => t.Reporter)
                    .Include(t => t.Project)
                    .FirstOrDefault(t => t.Id == id);

                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found", id);
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Task not found." });
                    }
                    
                    TempData["ErrorMessage"] = "Task not found.";
                    return RedirectToAction("Error", "Home");
                }

                // Get all users for assignee dropdown
                ViewBag.Users = _context.Users.ToList();

                // Return the partial view for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("Modals/_TaskDetailsPartial", task);
                }

                // Return full view for non-AJAX requests
                return View("TaskDetails", task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTaskDetails action: {Message}", ex.Message);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while retrieving task details." });
                }
                
                TempData["ErrorMessage"] = "An error occurred while retrieving task details.";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Board/UpdateTask
        [HttpPost]
        public async Task<IActionResult> UpdateTask(TaskItem taskModel, List<IFormFile> files)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return Json(new { success = false, message = "You must be logged in to update a task." });
                }

                // Find the existing task
                var task = await _context.Tasks.FindAsync(taskModel.Id);
                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found for update", taskModel.Id);
                    return Json(new { success = false, message = "Task not found." });
                }

                // Update task properties
                task.Title = taskModel.Title;
                task.Description = taskModel.Description;
                task.Status = taskModel.Status;
                task.Priority = taskModel.Priority;
                task.WorkType = taskModel.WorkType;
                task.AssigneeId = taskModel.AssigneeId;
                task.StoryPoints = taskModel.StoryPoints;
                task.StartDate = taskModel.StartDate;
                task.DueDate = taskModel.DueDate;

                // If status changed to Done, update completion date
                if (task.Status == TaskStatusEnum.Done && task.CompletionDate == null)
                {
                    task.CompletionDate = DateTime.Now;
                }
                else if (task.Status != TaskStatusEnum.Done)
                {
                    // Reset completion date if task is moved back from Done
                    task.CompletionDate = null;
                }

                // Handle file uploads
                if (files != null && files.Count > 0)
                {
                    // TODO: Implement file storage logic here
                    // For now, we'll just log that files were uploaded
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            _logger.LogInformation("File uploaded: {FileName}, Size: {FileSize} bytes", 
                                file.FileName, file.Length);
                            
                            // Example file handling (commented out for now)
                            // var fileName = Path.GetFileName(file.FileName);
                            // var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                            // using (var stream = new FileStream(filePath, FileMode.Create))
                            // {
                            //     await file.CopyToAsync(stream);
                            // }
                        }
                    }
                }

                // Save changes
                _context.Update(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Task updated successfully: TaskId={TaskId}", task.Id);
                return Json(new { success = true, message = "Task updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateTask action: {Message}", ex.Message);
                return Json(new { success = false, message = "An error occurred while updating the task." });
            }
        }

        // GET: Board/Search
        public IActionResult Search(string searchInput)
        {
            try
            {
                // Redirect if not logged in
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    return Request.Headers["X-Requested-With"] == "XMLHttpRequest" 
                        ? Json(new { success = false, message = "Please log in to search tasks." })
                        : RedirectToAction("Login", "Account");
                }

                // Set up our search results container
                var searchResults = new SearchResultsViewModel { Query = searchInput };
                
                // Look for matching tasks
                searchResults.Tasks = _context.Tasks
                    .Include(t => t.Assignee)
                    .Include(t => t.Reporter)
                    .Include(t => t.Project)
                    .Where(t => t.Title.Contains(searchInput) || 
                           t.Description.Contains(searchInput))
                    .ToList();

                // Get matching users
                searchResults.Users = _context.Users
                    .Where(u => u.Username.Contains(searchInput) || 
                           (u.Email != null && u.Email.Contains(searchInput)))
                    .ToList();

                // Grab any projects that match too
                searchResults.Projects = _context.Projects
                    .Where(p => p.Name.Contains(searchInput) || 
                           (p.Description != null && p.Description.Contains(searchInput)))
                    .ToList();

                return View("SearchResults", searchResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Search action: {Message}", ex.Message);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while searching tasks." });
                }
                
                TempData["ErrorMessage"] = "An error occurred while searching tasks.";
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
