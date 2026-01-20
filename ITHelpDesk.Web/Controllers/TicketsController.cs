using ITHelpDesk.Web.Data;
using ITHelpDesk.Web.Models;
using ITHelpDesk.Web.Models.ViewModels;
using ITHelpDesk.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Web.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly HelpDeskContext _context;
        private readonly IActiveDirectoryService _adService;
        private readonly ILogger<TicketsController> _logger;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;

        public TicketsController(HelpDeskContext context, IActiveDirectoryService adService,
            ILogger<TicketsController> logger, IEmailService emailService, IWebHostEnvironment environment)
        {
            _context = context;
            _adService = adService;
            _logger = logger;
            _emailService = emailService;
            _environment = environment;
        }

        // GET: Tickets
        public async Task<IActionResult> Index(string? searchTerm, string? statusFilter,
            string? priorityFilter, string? categoryFilter, string? assignedToFilter)
        {
            var query = _context.Tickets
                .Include(t => t.Comments)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    t.Description.Contains(searchTerm) ||
                    t.Comments.Any(c => c.CommentText.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                query = query.Where(t => t.Status == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(priorityFilter) && priorityFilter != "All")
            {
                query = query.Where(t => t.Priority == priorityFilter);
            }

            if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "All")
            {
                query = query.Where(t => t.Category == categoryFilter);
            }

            if (!string.IsNullOrWhiteSpace(assignedToFilter) && assignedToFilter != "All")
            {
                query = query.Where(t => t.AssignedTo == assignedToFilter);
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            // Calculate statistics
            var allTickets = await _context.Tickets.ToListAsync();
            var viewModel = new TicketListViewModel
            {
                Tickets = tickets,
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                PriorityFilter = priorityFilter,
                CategoryFilter = categoryFilter,
                AssignedToFilter = assignedToFilter,
                TotalTickets = allTickets.Count,
                NewTickets = allTickets.Count(t => t.Status == "New"),
                InProgressTickets = allTickets.Count(t => t.Status == "In Progress"),
                ResolvedTickets = allTickets.Count(t => t.Status == "Resolved"),
                ClosedTickets = allTickets.Count(t => t.Status == "Closed"),
                CriticalTickets = allTickets.Count(t => t.Priority == "Critical" && t.Status != "Closed"),
                HighPriorityTickets = allTickets.Count(t => t.Priority == "High" && t.Status != "Closed"),
                OverdueTickets = allTickets.Count(t => t.IsOverdue)
            };

            return View(viewModel);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(m => m.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            var comments = await _context.TicketComments
                .Where(c => c.TicketId == id)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();

            var history = await _context.TicketHistory
                .Where(h => h.TicketId == id)
                .OrderBy(h => h.ChangedDate)
                .ToListAsync();

            var viewModel = new TicketDetailsViewModel
            {
                Ticket = ticket,
                Comments = comments,
                History = history
            };

            return View(viewModel);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            var ticket = new Ticket
            {
                CreatedBy = User.Identity?.Name ?? "Unknown",
                CreatedDate = DateTime.Now,
                Status = "New",
                Priority = "Medium"
            };

            // Get user info from AD
            var adUser = _adService.GetUserByUsername(ticket.CreatedBy);
            if (adUser != null)
            {
                ticket.CreatedByEmail = adUser.Email;
            }

            ViewBag.ITStaff = _adService.GetITSupportUsers();
            return View(ticket);
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Priority,Category,RequestedFor,ComputerName,AssignedTo,DueDate")] Ticket ticket, List<IFormFile>? attachments)
        {
            _logger.LogInformation("Create POST method called for ticket: {Title}", ticket.Title);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                ViewBag.ITStaff = _adService.GetITSupportUsers();
                return View(ticket);
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid, proceeding to save ticket");

                // Set system fields
                ticket.CreatedBy = User.Identity?.Name ?? "Unknown";
                ticket.CreatedDate = DateTime.Now;
                ticket.Status = "New";

                // Get creator info from AD
                var creatorUser = _adService.GetUserByUsername(ticket.CreatedBy);
                if (creatorUser != null)
                {
                    ticket.CreatedByName = creatorUser.DisplayName;
                    ticket.CreatedByEmail = creatorUser.Email;
                }

                // Get requested for user info from AD if specified
                if (!string.IsNullOrWhiteSpace(ticket.RequestedFor))
                {
                    var requestedForUser = _adService.GetUserByUsername(ticket.RequestedFor);
                    if (requestedForUser != null)
                    {
                        ticket.RequestedForName = requestedForUser.DisplayName;
                        ticket.RequestedForEmail = requestedForUser.Email;
                    }
                }

                // Get assigned user info from AD if assigned
                if (!string.IsNullOrWhiteSpace(ticket.AssignedTo))
                {
                    var assignedUser = _adService.GetUserByUsername(ticket.AssignedTo);
                    if (assignedUser != null)
                    {
                        ticket.AssignedToName = assignedUser.DisplayName;
                        ticket.AssignedToEmail = assignedUser.Email;
                    }
                }

                _context.Add(ticket);
                await _context.SaveChangesAsync();

                // Add initial comment
                var initialComment = new TicketComment
                {
                    TicketId = ticket.TicketId,
                    CommentText = "Ticket created.",
                    CommentType = "System",
                    CreatedBy = ticket.CreatedBy,
                    CreatedByName = ticket.CreatedByName,
                    CreatedByEmail = ticket.CreatedByEmail,
                    CreatedDate = DateTime.Now
                };
                _context.TicketComments.Add(initialComment);

                // Log history
                await LogHistory(ticket.TicketId, "Status", null, "New", ticket.CreatedBy, ticket.CreatedByName);
                if (!string.IsNullOrWhiteSpace(ticket.AssignedTo))
                {
                    await LogHistory(ticket.TicketId, "AssignedTo", null, ticket.AssignedTo, ticket.CreatedBy, ticket.CreatedByName);
                }

                await _context.SaveChangesAsync();

                // Handle file attachments
                if (attachments != null && attachments.Any())
                {
                    await SaveAttachments(ticket.TicketId, attachments, ticket.CreatedBy, ticket.CreatedByName);
                }

                // Send email notifications
                try
                {
                    // Notify the person the ticket is for (if different from creator)
                    if (!string.IsNullOrWhiteSpace(ticket.RequestedForEmail) &&
                        ticket.RequestedForEmail != ticket.CreatedByEmail)
                    {
                        await _emailService.SendTicketCreatedEmailAsync(
                            ticket.TicketId,
                            ticket.RequestedForEmail,
                            ticket.RequestedFor ?? "User");
                    }

                    // Notify assigned IT staff
                    if (!string.IsNullOrWhiteSpace(ticket.AssignedToEmail))
                    {
                        await _emailService.SendTicketAssignedEmailAsync(
                            ticket.TicketId,
                            ticket.AssignedToEmail,
                            ticket.AssignedTo ?? "IT Staff",
                            ticket.CreatedBy);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email notifications for ticket #{TicketId}", ticket.TicketId);
                }

                _logger.LogInformation("Ticket #{TicketId} created successfully", ticket.TicketId);
                TempData["SuccessMessage"] = $"Ticket #{ticket.TicketId} created successfully.";
                return RedirectToAction(nameof(Details), new { id = ticket.TicketId });
            }

            _logger.LogWarning("Reached end of Create method without saving - ModelState was invalid");


            ViewBag.ITStaff = _adService.GetITSupportUsers();
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ViewBag.ITStaff = _adService.GetITSupportUsers();
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,Title,Description,Status,Priority,Category,RequestedFor,ComputerName,AssignedTo,DueDate,CreatedBy,CreatedByEmail,CreatedDate")] Ticket ticket, List<IFormFile>? attachments)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTicket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.TicketId == id);
                    if (existingTicket == null)
                    {
                        return NotFound();
                    }

                    var currentUser = User.Identity?.Name ?? "Unknown";

                    // Get current user display name from AD
                    var currentUserInfo = _adService.GetUserByUsername(currentUser);
                    var currentUserName = currentUserInfo?.DisplayName;

                    // Track changes and update assigned user email
                    await TrackChanges(existingTicket, ticket, currentUser, currentUserName);

                    // Get requested for user info from AD if changed
                    if (ticket.RequestedFor != existingTicket.RequestedFor && !string.IsNullOrWhiteSpace(ticket.RequestedFor))
                    {
                        var requestedForUser = _adService.GetUserByUsername(ticket.RequestedFor);
                        if (requestedForUser != null)
                        {
                            ticket.RequestedForName = requestedForUser.DisplayName;
                            ticket.RequestedForEmail = requestedForUser.Email;
                        }
                    }
                    else
                    {
                        // Preserve existing values if not changed
                        ticket.RequestedForName = existingTicket.RequestedForName;
                        ticket.RequestedForEmail = existingTicket.RequestedForEmail;
                    }

                    // Get assigned user info from AD if changed
                    if (ticket.AssignedTo != existingTicket.AssignedTo && !string.IsNullOrWhiteSpace(ticket.AssignedTo))
                    {
                        var assignedUser = _adService.GetUserByUsername(ticket.AssignedTo);
                        if (assignedUser != null)
                        {
                            ticket.AssignedToName = assignedUser.DisplayName;
                            ticket.AssignedToEmail = assignedUser.Email;
                        }
                    }
                    else
                    {
                        // Preserve existing values if not changed
                        ticket.AssignedToName = existingTicket.AssignedToName;
                        ticket.AssignedToEmail = existingTicket.AssignedToEmail;
                    }

                    // Update system fields
                    ticket.LastModifiedBy = currentUser;
                    ticket.LastModifiedByName = currentUserName;
                    ticket.LastModifiedDate = DateTime.Now;

                    // Preserve original creation info
                    ticket.CreatedByName = existingTicket.CreatedByName;
                    ticket.CreatedByEmail = existingTicket.CreatedByEmail;

                    // Set resolved/closed dates based on status
                    if (ticket.Status == "Resolved" && existingTicket.Status != "Resolved")
                    {
                        ticket.ResolvedDate = DateTime.Now;
                    }
                    else if (ticket.Status == "Closed" && existingTicket.Status != "Closed")
                    {
                        ticket.ClosedDate = DateTime.Now;
                        if (!ticket.ResolvedDate.HasValue)
                        {
                            ticket.ResolvedDate = DateTime.Now;
                        }
                    }

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

                    // Handle file attachments
                    if (attachments != null && attachments.Any())
                    {
                        await SaveAttachments(ticket.TicketId, attachments, currentUser, currentUserName);
                    }

                    // Send email notifications based on status changes
                    try
                    {
                        var statusChanged = ticket.Status != existingTicket.Status;
                        var assignmentChanged = ticket.AssignedTo != existingTicket.AssignedTo;

                        // If status changed to Resolved
                        if (ticket.Status == "Resolved" && existingTicket.Status != "Resolved")
                        {
                            if (!string.IsNullOrWhiteSpace(ticket.RequestedForEmail))
                            {
                                await _emailService.SendTicketResolvedEmailAsync(
                                    ticket.TicketId,
                                    ticket.RequestedForEmail,
                                    ticket.RequestedFor ?? "User");
                            }
                        }
                        // If status changed to Closed
                        else if (ticket.Status == "Closed" && existingTicket.Status != "Closed")
                        {
                            if (!string.IsNullOrWhiteSpace(ticket.RequestedForEmail))
                            {
                                await _emailService.SendTicketClosedEmailAsync(
                                    ticket.TicketId,
                                    ticket.RequestedForEmail,
                                    ticket.RequestedFor ?? "User");
                            }
                        }
                        // If assignment changed - notify new assignee
                        else if (assignmentChanged && !string.IsNullOrWhiteSpace(ticket.AssignedToEmail))
                        {
                            await _emailService.SendTicketAssignedEmailAsync(
                                ticket.TicketId,
                                ticket.AssignedToEmail,
                                ticket.AssignedTo ?? "IT Staff",
                                currentUser);
                        }
                        // For any other updates - notify requested for person
                        else if (!string.IsNullOrWhiteSpace(ticket.RequestedForEmail))
                        {
                            await _emailService.SendTicketUpdatedEmailAsync(
                                ticket.TicketId,
                                ticket.RequestedForEmail,
                                ticket.RequestedFor ?? "User",
                                "Your ticket has been updated.");
                        }

                        // Always notify assigned person about updates (unless they made the change or assignment just changed)
                        if (!assignmentChanged &&
                            !string.IsNullOrWhiteSpace(ticket.AssignedTo) &&
                            !string.IsNullOrWhiteSpace(ticket.AssignedToEmail) &&
                            !ticket.AssignedTo.Equals(currentUser, StringComparison.OrdinalIgnoreCase))
                        {
                            var updaterName = currentUserName ?? currentUser;
                            var updateDescription = statusChanged
                                ? $"{updaterName} changed the status to '{ticket.Status}'."
                                : $"{updaterName} updated the ticket.";

                            await _emailService.SendTicketUpdatedEmailAsync(
                                ticket.TicketId,
                                ticket.AssignedToEmail,
                                ticket.AssignedToName ?? ticket.AssignedTo,
                                updateDescription);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email notifications for ticket #{TicketId}", ticket.TicketId);
                    }

                    TempData["SuccessMessage"] = "Ticket updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = ticket.TicketId });
            }

            ViewBag.ITStaff = _adService.GetITSupportUsers();
            return View(ticket);
        }

        // POST: Tickets/AddComment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int ticketId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            var currentUser = User.Identity?.Name ?? "Unknown";
            var adUser = _adService.GetUserByUsername(currentUser);

            var comment = new TicketComment
            {
                TicketId = ticketId,
                CommentText = commentText,
                CommentType = "Comment",
                CreatedBy = currentUser,
                CreatedByName = adUser?.DisplayName,
                CreatedByEmail = adUser?.Email,
                CreatedDate = DateTime.Now
            };

            _context.TicketComments.Add(comment);

            // Update ticket modified info
            ticket.LastModifiedBy = currentUser;
            ticket.LastModifiedByName = adUser?.DisplayName;
            ticket.LastModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // Send email notification to assigned person if they didn't make the change
            try
            {
                if (!string.IsNullOrWhiteSpace(ticket.AssignedTo) &&
                    !string.IsNullOrWhiteSpace(ticket.AssignedToEmail) &&
                    !ticket.AssignedTo.Equals(currentUser, StringComparison.OrdinalIgnoreCase))
                {
                    var commenterName = adUser?.DisplayName ?? currentUser;
                    await _emailService.SendTicketUpdatedEmailAsync(
                        ticket.TicketId,
                        ticket.AssignedToEmail,
                        ticket.AssignedToName ?? ticket.AssignedTo,
                        $"{commenterName} added a comment: \"{commentText}\"");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send comment notification email for ticket #{TicketId}", ticketId);
            }

            TempData["SuccessMessage"] = "Comment added successfully.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // GET: Tickets/UserHistory
        public async Task<IActionResult> UserHistory(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                username = User.Identity?.Name ?? string.Empty;
            }

            var tickets = await _context.Tickets
                .Where(t => t.CreatedBy == username || t.AssignedTo == username)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            var adUser = _adService.GetUserByUsername(username);
            ViewBag.Username = username;
            ViewBag.DisplayName = adUser?.DisplayName ?? username;

            return View(tickets);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ticket deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketId == id);
        }

        private async Task TrackChanges(Ticket oldTicket, Ticket newTicket, string changedBy, string? changedByName = null)
        {
            if (oldTicket.Status != newTicket.Status)
            {
                await LogHistory(newTicket.TicketId, "Status", oldTicket.Status, newTicket.Status, changedBy, changedByName);

                // Add system comment for status change
                var comment = new TicketComment
                {
                    TicketId = newTicket.TicketId,
                    CommentText = $"Status changed from '{oldTicket.Status}' to '{newTicket.Status}'.",
                    CommentType = "Status Change",
                    CreatedBy = changedBy,
                    CreatedByName = changedByName,
                    CreatedDate = DateTime.Now
                };
                _context.TicketComments.Add(comment);
            }

            if (oldTicket.Priority != newTicket.Priority)
            {
                await LogHistory(newTicket.TicketId, "Priority", oldTicket.Priority, newTicket.Priority, changedBy, changedByName);
            }

            if (oldTicket.Category != newTicket.Category)
            {
                await LogHistory(newTicket.TicketId, "Category", oldTicket.Category, newTicket.Category, changedBy, changedByName);
            }

            if (oldTicket.AssignedTo != newTicket.AssignedTo)
            {
                await LogHistory(newTicket.TicketId, "AssignedTo", oldTicket.AssignedToName, newTicket.AssignedToName, changedBy, changedByName);

                // Add system comment for assignment
                var assignedToName = newTicket.AssignedToName ?? newTicket.AssignedTo ?? "Unassigned";
                var comment = new TicketComment
                {
                    TicketId = newTicket.TicketId,
                    CommentText = $"Ticket assigned to '{assignedToName}'.",
                    CommentType = "Assignment",
                    CreatedBy = changedBy,
                    CreatedByName = changedByName,
                    CreatedDate = DateTime.Now
                };
                _context.TicketComments.Add(comment);
            }

            if (oldTicket.Title != newTicket.Title)
            {
                await LogHistory(newTicket.TicketId, "Title", oldTicket.Title, newTicket.Title, changedBy, changedByName);
            }
        }

        private async Task LogHistory(int ticketId, string fieldChanged, string? oldValue, string? newValue, string changedBy, string? changedByName = null)
        {
            var history = new TicketHistory
            {
                TicketId = ticketId,
                FieldChanged = fieldChanged,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedBy = changedBy,
                ChangedByName = changedByName,
                ChangedDate = DateTime.Now
            };

            _context.TicketHistory.Add(history);
            await _context.SaveChangesAsync();
        }

        private async Task SaveAttachments(int ticketId, List<IFormFile> attachments, string uploadedBy, string? uploadedByName = null)
        {
            var uploadPath = Path.Combine(_environment.ContentRootPath, "App_Data", "Attachments", ticketId.ToString());
            Directory.CreateDirectory(uploadPath);

            foreach (var file in attachments)
            {
                if (file.Length > 0)
                {
                    // Validate file size (10MB limit)
                    if (file.Length > 10 * 1024 * 1024)
                    {
                        _logger.LogWarning("File {FileName} exceeds size limit", file.FileName);
                        continue;
                    }

                    // Generate safe filename
                    var fileName = Path.GetFileName(file.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                    var filePath = Path.Combine(uploadPath, uniqueFileName);

                    // Save file to disk
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Save attachment record to database
                    var attachment = new TicketAttachment
                    {
                        TicketId = ticketId,
                        FileName = fileName,
                        FilePath = filePath,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        UploadedBy = uploadedBy,
                        UploadedByName = uploadedByName,
                        UploadedDate = DateTime.Now
                    };

                    _context.TicketAttachments.Add(attachment);
                }
            }

            await _context.SaveChangesAsync();
        }

        // GET: Tickets/DownloadAttachment/5
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _context.TicketAttachments.FindAsync(id);
            if (attachment == null || !System.IO.File.Exists(attachment.FilePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(attachment.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
        }

        // POST: Tickets/DeleteAttachment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int id, int ticketId)
        {
            var attachment = await _context.TicketAttachments.FindAsync(id);
            if (attachment == null)
            {
                return NotFound();
            }

            // Delete file from disk
            if (System.IO.File.Exists(attachment.FilePath))
            {
                System.IO.File.Delete(attachment.FilePath);
            }

            // Delete database record
            _context.TicketAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Attachment deleted successfully.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }
    }
}
