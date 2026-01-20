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
        private readonly IWebHostEnvironment _environment;

        public EmailService(IConfiguration configuration, HelpDeskContext context, ILogger<EmailService> logger, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        public async Task SendTicketCreatedEmailAsync(int ticketId, string recipientEmail, string recipientName)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return;

            var subject = $"Help Desk Ticket #{ticketId} Created: {ticket.Title}";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto;'>
    <div style='text-align: center; padding: 20px 0; border-bottom: 2px solid #0033A0;'>
        <img src='cid:logo' alt='WN Global' style='max-width: 180px;' />
    </div>
    <div style='padding: 20px;'>
        <h2 style='color: #0033A0;'>Help Desk Ticket Created</h2>
        <p>Hello {recipientName},</p>
        <p>A help desk ticket has been created for you:</p>
        <table style='border-collapse: collapse; margin: 20px 0; width: 100%;'>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Ticket Number:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>#{ticketId}</td>
            </tr>
            <tr>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Title:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.Title}</td>
            </tr>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Priority:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.Priority}</td>
            </tr>
            <tr>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Status:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.Status}</td>
            </tr>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Created By:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.CreatedByName ?? ticket.CreatedBy}</td>
            </tr>
        </table>
        <p><strong>Description:</strong></p>
        <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #0033A0; margin: 10px 0;'>
            <p>{ticket.Description}</p>
        </div>
        <p>You will receive updates as the ticket progresses.</p>
    </div>
    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;' />
    <div style='padding: 0 20px 20px 20px; color: #666; font-size: 14px;'>
        <p>
            Thank you,<br/>
            <strong>IT Help Desk</strong><br/>
            <a href='mailto:it@wn-global.com' style='color: #0033A0;'>it@wn-global.com</a>
        </p>
    </div>
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
<body style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto;'>
    <div style='text-align: center; padding: 20px 0; border-bottom: 2px solid #0033A0;'>
        <img src='cid:logo' alt='WN Global' style='max-width: 180px;' />
    </div>
    <div style='padding: 20px;'>
        <h2 style='color: #0033A0;'>Help Desk Ticket Updated</h2>
        <p>Hello {recipientName},</p>
        <p>Your help desk ticket has been updated:</p>
        <table style='border-collapse: collapse; margin: 20px 0; width: 100%;'>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Ticket Number:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>#{ticketId}</td>
            </tr>
            <tr>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Title:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.Title}</td>
            </tr>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Status:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.Status}</td>
            </tr>
        </table>
        <p><strong>Update:</strong></p>
        <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #0033A0; margin: 10px 0;'>
            <p>{updateDescription}</p>
        </div>
    </div>
    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;' />
    <div style='padding: 0 20px 20px 20px; color: #666; font-size: 14px;'>
        <p>
            Thank you,<br/>
            <strong>IT Help Desk</strong><br/>
            <a href='mailto:it@wn-global.com' style='color: #0033A0;'>it@wn-global.com</a>
        </p>
    </div>
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
<body style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto;'>
    <div style='text-align: center; padding: 20px 0; border-bottom: 2px solid #0033A0;'>
        <img src='cid:logo' alt='WN Global' style='max-width: 180px;' />
    </div>
    <div style='padding: 20px;'>
        <h2 style='color: #0033A0;'>Help Desk Ticket Closed</h2>
        <p>Hello {recipientName},</p>
        <p>Your help desk ticket has been closed:</p>
        <table style='border-collapse: collapse; margin: 20px 0; width: 100%;'>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Ticket Number:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>#{ticketId}</td>
            </tr>
            <tr>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Title:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.Title}</td>
            </tr>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Closed Date:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.ClosedDate?.ToString("MM/dd/yyyy hh:mm tt")}</td>
            </tr>
        </table>
        <p>If you have any questions or need to reopen this ticket, please contact the IT Help Desk.</p>
    </div>
    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;' />
    <div style='padding: 0 20px 20px 20px; color: #666; font-size: 14px;'>
        <p>
            Thank you,<br/>
            <strong>IT Help Desk</strong><br/>
            <a href='mailto:it@wn-global.com' style='color: #0033A0;'>it@wn-global.com</a>
        </p>
    </div>
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
<body style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto;'>
    <div style='text-align: center; padding: 20px 0; border-bottom: 2px solid #0033A0;'>
        <img src='cid:logo' alt='WN Global' style='max-width: 180px;' />
    </div>
    <div style='padding: 20px;'>
        <h2 style='color: #0033A0;'>Help Desk Ticket Resolved</h2>
        <p>Hello {recipientName},</p>
        <p>Your help desk ticket has been marked as resolved:</p>
        <table style='border-collapse: collapse; margin: 20px 0; width: 100%;'>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Ticket Number:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>#{ticketId}</td>
            </tr>
            <tr>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Title:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.Title}</td>
            </tr>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Resolved Date:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.ResolvedDate?.ToString("MM/dd/yyyy hh:mm tt")}</td>
            </tr>
        </table>
        <p>If this issue is not fully resolved or you have additional questions, please contact the IT Help Desk.</p>
    </div>
    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;' />
    <div style='padding: 0 20px 20px 20px; color: #666; font-size: 14px;'>
        <p>
            Thank you,<br/>
            <strong>IT Help Desk</strong><br/>
            <a href='mailto:it@wn-global.com' style='color: #0033A0;'>it@wn-global.com</a>
        </p>
    </div>
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
<body style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto;'>
    <div style='text-align: center; padding: 20px 0; border-bottom: 2px solid #0033A0;'>
        <img src='cid:logo' alt='WN Global' style='max-width: 180px;' />
    </div>
    <div style='padding: 20px;'>
        <h2 style='color: #0033A0;'>Help Desk Ticket Assigned</h2>
        <p>Hello {recipientName},</p>
        <p>A help desk ticket has been assigned to you by {assignedBy}:</p>
        <table style='border-collapse: collapse; margin: 20px 0; width: 100%;'>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Ticket Number:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>#{ticketId}</td>
            </tr>
            <tr>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Title:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.Title}</td>
            </tr>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Priority:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.Priority}</td>
            </tr>
            <tr>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Status:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.Status}</td>
            </tr>
            <tr style='background-color: #f5f5f5;'>
                <td style='padding: 10px; font-weight: bold; border: 1px solid #ddd;'>Requested For:</td>
                <td style='padding: 10px; border: 1px solid #ddd;'>{ticket.RequestedForName ?? ticket.RequestedFor ?? "N/A"}</td>
            </tr>
        </table>
        <p><strong>Description:</strong></p>
        <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #0033A0; margin: 10px 0;'>
            <p>{ticket.Description}</p>
        </div>
    </div>
    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;' />
    <div style='padding: 0 20px 20px 20px; color: #666; font-size: 14px;'>
        <p>
            Thank you,<br/>
            <strong>IT Help Desk</strong><br/>
            <a href='mailto:it@wn-global.com' style='color: #0033A0;'>it@wn-global.com</a>
        </p>
    </div>
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

                // Attach logo if it exists
                var logoPath = Path.Combine(_environment.WebRootPath, "images", "wn-global-logo.png");
                if (File.Exists(logoPath))
                {
                    var logo = new Attachment(logoPath);
                    logo.ContentId = "logo";
                    logo.ContentDisposition.Inline = true;
                    logo.ContentDisposition.DispositionType = System.Net.Mime.DispositionTypeNames.Inline;
                    mailMessage.Attachments.Add(logo);
                }

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
