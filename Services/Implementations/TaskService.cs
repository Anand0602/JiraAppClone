using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JiraApp.Models.Database;
using JiraApp.Models.Tasks;
using JiraApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraApp.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly JiraAppContext _context;

        public TaskService(JiraAppContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Reporter)
                .Include(t => t.Project)
                .ToListAsync();
        }

        public async Task<TaskItem> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Reporter)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem> UpdateTaskAsync(TaskItem task)
        {
            // Detach any navigation properties to prevent tracking issues
            task.Assignee = null;
            task.Reporter = null;
            task.Project = null;
            
            // Get the existing task to ensure we're not losing data
            var existingTask = await _context.Tasks.FindAsync(task.Id);
            if (existingTask != null)
            {
                // Update the existing task with the new values
                _context.Entry(existingTask).CurrentValues.SetValues(task);
                await _context.SaveChangesAsync();
                return existingTask;
            }
            else
            {
                // If task doesn't exist, update as before
                _context.Entry(task).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return task;
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(int projectId)
        {
            return await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Reporter)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByAssigneeIdAsync(int assigneeId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Reporter)
                .Where(t => t.AssigneeId == assigneeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByReporterIdAsync(int reporterId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .Where(t => t.ReporterId == reporterId)
                .ToListAsync();
        }
    }
}
