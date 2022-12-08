using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Notification
    {
        // Primary KEY
        public int Id { get; set; }

        // Foreign Key
        public int? ProjectId { get; set; }

        // Foreign Key
        public int? TicketId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Message { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        // Foreign Key
        [Required]
        public string? SenderId { get; set; }

        // Foreign Key
        [Required]
        public string? RecipientId { get; set; }

        // Foreign Key
        public int NotificationTypeId { get; set; }

        [DisplayName("Has been Read")]
        public bool HasBeenViewed { get; set; }

        // NAVIGATION PROPERTIES

        public virtual NotificationType? NotificationType { get; set; }

        public virtual Ticket? Ticket { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Sender { get; set; }

        public virtual BTUser? Recipient { get; set; }
    }
}
