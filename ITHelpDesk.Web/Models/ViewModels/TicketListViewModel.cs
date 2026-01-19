namespace ITHelpDesk.Web.Models.ViewModels
{
    public class TicketListViewModel
    {
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? PriorityFilter { get; set; }
        public string? CategoryFilter { get; set; }
        public string? AssignedToFilter { get; set; }

        // Dashboard statistics
        public int TotalTickets { get; set; }
        public int NewTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int CriticalTickets { get; set; }
        public int HighPriorityTickets { get; set; }
        public int OverdueTickets { get; set; }
    }
}
