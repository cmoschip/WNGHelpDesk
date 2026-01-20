using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Web.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(255)]
        [Display(Name = "Ticket Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = "New";

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium";

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(255)]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(255)]
        public string? CreatedByEmail { get; set; }

        [Display(Name = "Date Created")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(255)]
        [Display(Name = "Requested For")]
        public string? RequestedFor { get; set; }

        [StringLength(255)]
        public string? RequestedForEmail { get; set; }

        [StringLength(255)]
        [Display(Name = "Computer Name")]
        public string? ComputerName { get; set; }

        [StringLength(255)]
        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        [StringLength(255)]
        public string? AssignedToEmail { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Resolved Date")]
        public DateTime? ResolvedDate { get; set; }

        [Display(Name = "Closed Date")]
        public DateTime? ClosedDate { get; set; }

        [StringLength(255)]
        public string? LastModifiedBy { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        // Navigation properties
        public virtual ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public virtual ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();

        [NotMapped]
        public bool IsOverdue => DueDate.HasValue &&
                                 DueDate.Value < DateTime.Now &&
                                 Status != "Closed" &&
                                 Status != "Resolved";
    }
}
