using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelpDesk.Web.Models
{
    public class TicketHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        [StringLength(100)]
        public string FieldChanged { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        [Required]
        [StringLength(255)]
        public string ChangedBy { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ChangedByName { get; set; }

        public DateTime ChangedDate { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }
    }
}
