using System;
using System.ComponentModel.DataAnnotations;
using JiraApp.Models.Projects;
using JiraApp.Models.Authentication;

namespace JiraApp.Models.Tasks
{
    // Enum for task status
    public enum TaskStatusEnum
    {
        ToDo,
        InProgress,
        Done
    }

    // Enum for work type
    public enum WorkType
    {
        Story,
        Bug,
        Task
    }

    // Enum for priority
    public enum Priority
    {
        Low,
        Medium,
        High
    }

    public class TaskItem
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        // Using enum for status
        public TaskStatusEnum Status { get; set; }
        
        // Using enum for work type
        public WorkType WorkType { get; set; }
        
        // Project relationship
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        
        public int ReporterId { get; set; }
        public virtual User Reporter { get; set; }
        
        public int? AssigneeId { get; set; }
        public virtual User Assignee { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public int? StoryPoints { get; set; }
        
        // Using enum for priority
        public Priority Priority { get; set; } = Priority.Medium;
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public DateTime? CompletionDate { get; set; }

        // Constructor to initialize properties
        public TaskItem()
        {
            Title = "";
            Description = "";
            Status = TaskStatusEnum.ToDo;
            WorkType = WorkType.Task;
            CreatedAt = DateTime.Now;
        }
    }
}
