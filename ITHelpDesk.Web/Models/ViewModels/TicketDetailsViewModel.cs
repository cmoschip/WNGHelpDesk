namespace ITHelpDesk.Web.Models.ViewModels
{
    public class TicketDetailsViewModel
    {
        public Ticket Ticket { get; set; } = new Ticket();
        public List<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public List<TicketHistory> History { get; set; } = new List<TicketHistory>();
        public string? NewComment { get; set; }
    }
}
