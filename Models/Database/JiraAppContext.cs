using Microsoft.EntityFrameworkCore;
using JiraApp.Models.Authentication;
using JiraApp.Models.Tasks;
using JiraApp.Models.Projects;

namespace JiraApp.Models.Database
{
    public class JiraAppContext : DbContext
    {
        public JiraAppContext(DbContextOptions<JiraAppContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Reporter)
                .WithMany(u => u.ReportedTasks)
                .HasForeignKey(t => t.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Project-Task relationship
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Admin user will be created through a database query instead of seeding
        }
    }
}
