using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        //Primary Key
        public int Id { get; set; }

        //Foreign Key
        public int TicketId { get; set; }

        [DisplayName("Updated Ticket Propery")]
        public string? PropertyName { get; set; }

        [DisplayName("Description of Change")]
        [StringLength(5000)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        [DisplayName("Previous Value")]
        public string? OldValue { get; set; }

        [DisplayName("Current Value")]
        public string? NewValue { get; set; }

        [Required]
        public string? UserId { get; set; }

        // NAVIAGATION PROPERTIES
      
        public virtual Ticket? Ticket { get; set; }

     
        public virtual BTUser? User { get; set; }
    }
}
