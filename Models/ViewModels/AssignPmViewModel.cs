using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class AssignPmViewModel
    {
        public Project? Project { get; set; }
        public SelectList? PmList { get; set; }
        public string? PMId { get; set; }
    }
}
