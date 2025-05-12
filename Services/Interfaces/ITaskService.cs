using System.Collections.Generic;
using System.Threading.Tasks;
using JiraApp.Models.Tasks;

namespace JiraApp.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetAllTasksAsync();
        Task<TaskItem> GetTaskByIdAsync(int id);
        Task<TaskItem> CreateTaskAsync(TaskItem task);
        Task<TaskItem> UpdateTaskAsync(TaskItem task);
        Task<bool> DeleteTaskAsync(int id);
        Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(int projectId);
        Task<IEnumerable<TaskItem>> GetTasksByAssigneeIdAsync(int assigneeId);
        Task<IEnumerable<TaskItem>> GetTasksByReporterIdAsync(int reporterId);
    }
}
