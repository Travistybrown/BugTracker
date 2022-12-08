using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketPriority
    {
        //Primary Key
        public int Id { get; set; }

        [Required]
        [DisplayName("Ticket Priority Name")]
        public string Name { get; set; }
    }
}
