using System;
using System.Collections.Generic;
using JiraApp.Models.Projects;

namespace JiraApp.DTOs
{
    public class ProjectDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public List<SprintDTO> Sprints { get; set; } = new List<SprintDTO>();
    }

    public class SprintDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
    }
}
