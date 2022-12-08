using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketComment
    {
        // Primary Key
        public int Id { get; set; }

        [Required]
        [DisplayName("Member Comment")]
        [StringLength(2000)]
        public string? Comment { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Created Date")]
        public DateTime Created { get; set; }

        // Foreing Key
        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }


        // NAVIAGATION PROPERTIES

        [DisplayName("Ticket")]
        public virtual Ticket? Ticket { get; set; }

        [DisplayName("Team Member")]
        public virtual BTUser? User { get; set; }

    }
}
