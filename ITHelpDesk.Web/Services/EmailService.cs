using ITHelpDesk.Web.Data;
using ITHelpDesk.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace ITHelpDesk.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HelpDeskContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, HelpDeskContext context, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public async Task SendTicketCreatedEmailAsync(int ticketId, string recipientEmail, string recipientName)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return;

            var subject = $"Help Desk Ticket #{ticketId} Created: {ticket.Title}";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Help Desk Ticket Created</h2>
    <p>Hello {recipientName},</p>
    <p>A help desk ticket has been created for you:</p>
    <table style='border-collapse: collapse; margin: 20px 0;'>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Ticket Number:</td>
            <td style='padding: 8px;'>#{ticketId}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Title:</td>
            <td style='padding: 8px;'>{ticket.Title}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Priority:</td>
            <td style='padding: 8px;'>{ticket.Priority}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Status:</td>
            <td style='padding: 8px;'>{ticket.Status}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Created By:</td>
            <td style='padding: 8px;'>{ticket.CreatedBy}</td>
        </tr>
    </table>
    <p><strong>Description:</strong></p>
    <p>{ticket.Description}</p>
    <p>You will receive updates as the ticket progresses.</p>
    <p>Thank you,<br/>IT Help Desk</p>
</body>
</html>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        public async Task SendTicketUpdatedEmailAsync(int ticketId, string recipientEmail, string recipientName, string updateDescription)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return;

            var subject = $"Help Desk Ticket #{ticketId} Updated: {ticket.Title}";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Help Desk Ticket Updated</h2>
    <p>Hello {recipientName},</p>
    <p>Your help desk ticket has been updated:</p>
    <table style='border-collapse: collapse; margin: 20px 0;'>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Ticket Number:</td>
            <td style='padding: 8px;'>#{ticketId}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Title:</td>
            <td style='padding: 8px;'>{ticket.Title}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Status:</td>
            <td style='padding: 8px;'>{ticket.Status}</td>
        </tr>
    </table>
    <p><strong>Update:</strong></p>
    <p>{updateDescription}</p>
    <p>Thank you,<br/>IT Help Desk</p>
</body>
</html>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        public async Task SendTicketClosedEmailAsync(int ticketId, string recipientEmail, string recipientName)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return;

            var subject = $"Help Desk Ticket #{ticketId} Closed: {ticket.Title}";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Help Desk Ticket Closed</h2>
    <p>Hello {recipientName},</p>
    <p>Your help desk ticket has been closed:</p>
    <table style='border-collapse: collapse; margin: 20px 0;'>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Ticket Number:</td>
            <td style='padding: 8px;'>#{ticketId}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Title:</td>
            <td style='padding: 8px;'>{ticket.Title}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Closed Date:</td>
            <td style='padding: 8px;'>{ticket.ClosedDate?.ToString("MM/dd/yyyy hh:mm tt")}</td>
        </tr>
    </table>
    <p>If you have any questions or need to reopen this ticket, please contact the IT Help Desk.</p>
    <p>Thank you,<br/>IT Help Desk</p>
</body>
</html>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        public async Task SendTicketResolvedEmailAsync(int ticketId, string recipientEmail, string recipientName)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return;

            var subject = $"Help Desk Ticket #{ticketId} Resolved: {ticket.Title}";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Help Desk Ticket Resolved</h2>
    <p>Hello {recipientName},</p>
    <p>Your help desk ticket has been marked as resolved:</p>
    <table style='border-collapse: collapse; margin: 20px 0;'>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Ticket Number:</td>
            <td style='padding: 8px;'>#{ticketId}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Title:</td>
            <td style='padding: 8px;'>{ticket.Title}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Resolved Date:</td>
            <td style='padding: 8px;'>{ticket.ResolvedDate?.ToString("MM/dd/yyyy hh:mm tt")}</td>
        </tr>
    </table>
    <p>If this issue is not fully resolved or you have additional questions, please contact the IT Help Desk.</p>
    <p>Thank you,<br/>IT Help Desk</p>
</body>
</html>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        public async Task SendTicketAssignedEmailAsync(int ticketId, string recipientEmail, string recipientName, string assignedBy)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return;

            var subject = $"Help Desk Ticket #{ticketId} Assigned to You: {ticket.Title}";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Help Desk Ticket Assigned</h2>
    <p>Hello {recipientName},</p>
    <p>A help desk ticket has been assigned to you by {assignedBy}:</p>
    <table style='border-collapse: collapse; margin: 20px 0;'>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Ticket Number:</td>
            <td style='padding: 8px;'>#{ticketId}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Title:</td>
            <td style='padding: 8px;'>{ticket.Title}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Priority:</td>
            <td style='padding: 8px;'>{ticket.Priority}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Status:</td>
            <td style='padding: 8px;'>{ticket.Status}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold;'>Requested For:</td>
            <td style='padding: 8px;'>{ticket.RequestedFor ?? "N/A"}</td>
        </tr>
    </table>
    <p><strong>Description:</strong></p>
    <p>{ticket.Description}</p>
    <p>Thank you,<br/>IT Help Desk</p>
</body>
</html>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "25");
                var fromEmail = _configuration["Email:FromAddress"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogWarning("Email configuration is missing. Email not sent.");
                    return;
                }

                using var smtpClient = new SmtpClient(smtpServer, smtpPort);
                smtpClient.EnableSsl = false; // No SSL for internal relay
                smtpClient.UseDefaultCredentials = true; // No authentication needed
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "IT Help Desk"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}. Subject: {Subject}", toEmail, subject);
                // Don't throw - we don't want email failures to break the application flow
            }
        }
    }
}
