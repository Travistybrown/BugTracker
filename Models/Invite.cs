using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Invite
    {
        // Primary Key
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Sent")]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Joined")]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken    { get; set; }

        // Foreign Key
        public int CompanyId { get; set; }

        // Foreign Key
        public int ProjectId { get; set; }

        // Foreign Key
        [Required]
        public string? InvitorId { get; set; }

        // Foreign Key
        public string? InviteeId { get; set; }

        // Foreign Key
        [Required]
        [DisplayName("Email")]
        public string? InviteeEmail { get; set; }

        // Foreign Key
        [Required]
        [DisplayName("First Name")]
        public string? InviteeFirstName { get; set; }

        // Foreign Key
        [Required]
        [DisplayName("Last Name")]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        public bool IsValid { get; set; }


        // Navigation Properties

        public virtual Company? Company { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Invitor { get; set; }

        public virtual BTUser? Invitee { get; set; }
    }
}
