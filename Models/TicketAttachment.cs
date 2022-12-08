using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
           //Primary Key
        public int Id { get; set; }

        [DisplayName("File Description")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Added")]
        public DateTime Created { get; set; }

        //Foreign Key
        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [NotMapped]
        public IFormFile? FormFile { get; set; }

        [DisplayName("FileName")]
        public string? FileName { get; set; }

        [DisplayName("FileData")]
        public byte[]? FileData { get; set; }

        [DisplayName("File Extension")]
        public string? FileType { get; set; }

        // NAVIAGATION PROPERTIES

        [DisplayName("Ticket")]
        public virtual Ticket? Ticket { get; set; }

        [DisplayName("Team Member")]
        public virtual BTUser? User { get; set; }
    }
}
