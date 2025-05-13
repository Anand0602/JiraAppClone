using System.Collections.Generic;
using JiraApp.Models.Tasks;
using JiraApp.Models.Authentication;
using JiraApp.Models.Projects;

namespace JiraApp.ViewModels.Board
{
    public class SearchResultsViewModel
    {
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public List<User> Users { get; set; } = new List<User>();
        public List<Project> Projects { get; set; } = new List<Project>();
        public string Query { get; set; }
    }
}
