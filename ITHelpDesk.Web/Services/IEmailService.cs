namespace ITHelpDesk.Web.Services
{
    public interface IEmailService
    {
        Task SendTicketCreatedEmailAsync(int ticketId, string recipientEmail, string recipientName);
        Task SendTicketUpdatedEmailAsync(int ticketId, string recipientEmail, string recipientName, string updateDescription);
        Task SendTicketClosedEmailAsync(int ticketId, string recipientEmail, string recipientName);
        Task SendTicketResolvedEmailAsync(int ticketId, string recipientEmail, string recipientName);
        Task SendTicketAssignedEmailAsync(int ticketId, string recipientEmail, string recipientName, string assignedBy);
    }
}
