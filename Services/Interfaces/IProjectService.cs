using System.Collections.Generic;
using System.Threading.Tasks;
using JiraApp.Models.Projects;

namespace JiraApp.Services.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project> GetProjectByIdAsync(int id);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task<bool> DeleteProjectAsync(int id);
        Task<IEnumerable<Sprint>> GetSprintsByProjectIdAsync(int projectId);
    }
}
