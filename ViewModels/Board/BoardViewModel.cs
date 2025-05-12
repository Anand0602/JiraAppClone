using System.Collections.Generic;
using JiraApp.Models.Authentication;
using JiraApp.Models.Tasks;
using JiraApp.Models.Projects;

namespace JiraApp.ViewModels.Board
{
    // View model for the board view
    public class BoardViewModel
    {
        public string CurrentUserRole { get; set; } = "";
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public List<User> Users { get; set; } = new List<User>();
        public List<Project> Projects { get; set; } = new List<Project>();
    }
    
    // View model for user tasks view
    public class UserTasksViewModel
    {
        public User User { get; set; } = new User();
        public List<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public List<TaskItem> ReportedTasks { get; set; } = new List<TaskItem>();
    }
}
