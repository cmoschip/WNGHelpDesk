using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Web.Models
{
    public class TicketComment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Comment text is required")]
        [Display(Name = "Comment")]
        public string CommentText { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CommentType { get; set; } = "Comment";

        [Required]
        [StringLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(255)]
        public string? CreatedByEmail { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsInternal { get; set; } = false;

        // Navigation property
        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }
    }
}
