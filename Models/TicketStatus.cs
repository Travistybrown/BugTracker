using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketStatus
    {
        //Primary Key
        public int Id { get; set; }

        [Required]
        [DisplayName("Status Name")]
        public string? Name { get; set; }
    }
}
