using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Web.Models
{
    public class TicketAttachment
    {
        [Key]
        public int TicketAttachmentId { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "File Name")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [Display(Name = "File Size (bytes)")]
        public long FileSize { get; set; }

        [StringLength(100)]
        [Display(Name = "Content Type")]
        public string? ContentType { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Uploaded By")]
        public string UploadedBy { get; set; } = string.Empty;

        [StringLength(255)]
        public string? UploadedByName { get; set; }

        [Required]
        [Display(Name = "Uploaded Date")]
        public DateTime UploadedDate { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }
    }
}
