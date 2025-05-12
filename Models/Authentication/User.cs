using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JiraApp.Models.Tasks;

namespace JiraApp.Models.Authentication
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; } // Plain text as requested (not recommended for production)
        
        [Required]
        public string Role { get; set; } // "Admin" or "User"
        
        [EmailAddress]
        public string? Email { get; set; }
        
        // Navigation properties
        public virtual ICollection<TaskItem> AssignedTasks { get; set; }
        public virtual ICollection<TaskItem> ReportedTasks { get; set; }
        
        public User()
        {
            AssignedTasks = new HashSet<TaskItem>();
            ReportedTasks = new HashSet<TaskItem>();
        }
    }
}
