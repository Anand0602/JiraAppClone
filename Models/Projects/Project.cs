using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JiraApp.Models.Tasks;

namespace JiraApp.Models.Projects
{
    // Enum for project status
    public enum ProjectStatus
    {
        Active,
        Completed,
        OnHold
    }

    public class Project
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        // Using enum for status
        public ProjectStatus Status { get; set; }
        
        // When the project was created
        public DateTime CreatedDate { get; set; }
        
        // Navigation property for tasks in this project
        public virtual ICollection<TaskItem> Tasks { get; set; }
        
        // Constructor to initialize collection
        public Project()
        {
            Tasks = new HashSet<TaskItem>();
            CreatedDate = DateTime.Now;
            Status = ProjectStatus.Active; // Default status
            Name = "";
            Description = "";
        }
    }
}
