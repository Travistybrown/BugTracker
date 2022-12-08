using BugTracker.Models.ViewModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class Project /*: AssignPmViewModel*/
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Key
        public int CompanyId { get; set; }

        [Required]
        [DisplayName("Project Name")]
        public string? Name { get; set; }

        [Required]
        [DisplayName("Project Description")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Project Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Project End Date")]
        public DateTime EndDate { get; set; }

        // Foreign Key
        public int ProjectPriorityId { get; set; }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }

        public byte[]? ImageFileData { get; set; }

        public string? ImageFileType { get; set; }

        public bool Archived { get; set; }

        // Navigation Properties

        public virtual Company? Company{ get; set; }

        public virtual ProjectPriority? ProjectPriority { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

      
    }
}
