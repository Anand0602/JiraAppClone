using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JiraApp.Models.Tasks;

namespace JiraApp.Models.Projects
{
    public class Sprint
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        
        public bool IsActive { get; set; }
        
        // Project relationship
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        
        // Navigation property
        public virtual ICollection<TaskItem> Tasks { get; set; }
        
        public Sprint()
        {
            Tasks = new HashSet<TaskItem>();
        }
    }
}
