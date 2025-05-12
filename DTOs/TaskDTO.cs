using System;
using JiraApp.Models.Tasks;

namespace JiraApp.DTOs
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? AssigneeId { get; set; }
        public string AssigneeName { get; set; }
        public int ReporterId { get; set; }
        public string ReporterName { get; set; }
        public TaskStatusEnum Status { get; set; }
        public string StatusDisplay { get; set; }
        public WorkType WorkType { get; set; }
        public string WorkTypeDisplay { get; set; }
        public Priority Priority { get; set; }
        public string PriorityDisplay { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}
