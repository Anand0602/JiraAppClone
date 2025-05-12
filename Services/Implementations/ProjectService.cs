using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JiraApp.Models.Database;
using JiraApp.Models.Projects;
using JiraApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraApp.Services.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly JiraAppContext _context;

        public ProjectService(JiraAppContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<Project> GetProjectByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Sprint>> GetSprintsByProjectIdAsync(int projectId)
        {
            return await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .ToListAsync();
        }
    }
}
