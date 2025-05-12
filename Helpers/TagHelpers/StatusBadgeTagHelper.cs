using JiraApp.Models.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JiraApp.Helpers.TagHelpers
{
    [HtmlTargetElement("status-badge")]
    public class StatusBadgeTagHelper : TagHelper
    {
        [HtmlAttributeName("status")]
        public TaskStatusEnum Status { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.Attributes.SetAttribute("class", GetBadgeClass());
            output.Content.SetContent(GetStatusText());
        }

        private string GetBadgeClass()
        {
            return Status switch
            {
                TaskStatusEnum.ToDo => "badge bg-secondary",
                TaskStatusEnum.InProgress => "badge bg-primary",
                TaskStatusEnum.Done => "badge bg-success",
                _ => "badge bg-secondary"
            };
        }

        private string GetStatusText()
        {
            return Status switch
            {
                TaskStatusEnum.ToDo => "To Do",
                TaskStatusEnum.InProgress => "In Progress",
                TaskStatusEnum.Done => "Done",
                _ => Status.ToString()
            };
        }
    }

    [HtmlTargetElement("priority-badge")]
    public class PriorityBadgeTagHelper : TagHelper
    {
        [HtmlAttributeName("priority")]
        public Priority Priority { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.Attributes.SetAttribute("class", GetBadgeClass());
            output.Content.SetContent(GetPriorityText());
        }

        private string GetBadgeClass()
        {
            return Priority switch
            {
                Priority.Low => "badge bg-info",
                Priority.Medium => "badge bg-warning",
                Priority.High => "badge bg-danger",
                _ => "badge bg-secondary"
            };
        }

        private string GetPriorityText()
        {
            return Priority.ToString();
        }
    }

    [HtmlTargetElement("work-type-badge")]
    public class WorkTypeBadgeTagHelper : TagHelper
    {
        [HtmlAttributeName("work-type")]
        public WorkType WorkType { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.Attributes.SetAttribute("class", GetBadgeClass());
            output.Content.SetContent(GetWorkTypeText());
        }

        private string GetBadgeClass()
        {
            return WorkType switch
            {
                WorkType.Story => "badge bg-primary",
                WorkType.Bug => "badge bg-danger",
                WorkType.Task => "badge bg-info",
                _ => "badge bg-secondary"
            };
        }

        private string GetWorkTypeText()
        {
            return WorkType.ToString();
        }
    }
}
