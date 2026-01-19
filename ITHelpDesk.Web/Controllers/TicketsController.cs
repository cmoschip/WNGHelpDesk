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

        public TicketsController(HelpDeskContext context, IActiveDirectoryService adService)
        {
            _context = context;
            _adService = adService;
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
        public async Task<IActionResult> Create([Bind("Title,Description,Priority,Category,RequestedFor,AssignedTo,DueDate")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                // Set system fields
                ticket.CreatedBy = User.Identity?.Name ?? "Unknown";
                ticket.CreatedDate = DateTime.Now;
                ticket.Status = "New";

                // Get creator email from AD
                var creatorUser = _adService.GetUserByUsername(ticket.CreatedBy);
                if (creatorUser != null)
                {
                    ticket.CreatedByEmail = creatorUser.Email;
                }

                // Get requested for user email from AD if specified
                if (!string.IsNullOrWhiteSpace(ticket.RequestedFor))
                {
                    var requestedForUser = _adService.GetUserByUsername(ticket.RequestedFor);
                    if (requestedForUser != null)
                    {
                        ticket.RequestedForEmail = requestedForUser.Email;
                    }
                }

                // Get assigned user email from AD if assigned
                if (!string.IsNullOrWhiteSpace(ticket.AssignedTo))
                {
                    var assignedUser = _adService.GetUserByUsername(ticket.AssignedTo);
                    if (assignedUser != null)
                    {
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
                    CreatedByEmail = ticket.CreatedByEmail,
                    CreatedDate = DateTime.Now
                };
                _context.TicketComments.Add(initialComment);

                // Log history
                await LogHistory(ticket.TicketId, "Status", null, "New", ticket.CreatedBy);
                if (!string.IsNullOrWhiteSpace(ticket.AssignedTo))
                {
                    await LogHistory(ticket.TicketId, "AssignedTo", null, ticket.AssignedTo, ticket.CreatedBy);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Ticket #{ticket.TicketId} created successfully.";
                return RedirectToAction(nameof(Details), new { id = ticket.TicketId });
            }

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
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,Title,Description,Status,Priority,Category,RequestedFor,AssignedTo,DueDate,CreatedBy,CreatedByEmail,CreatedDate")] Ticket ticket)
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

                    // Track changes and update assigned user email
                    await TrackChanges(existingTicket, ticket, currentUser);

                    // Get requested for user email from AD if changed
                    if (ticket.RequestedFor != existingTicket.RequestedFor && !string.IsNullOrWhiteSpace(ticket.RequestedFor))
                    {
                        var requestedForUser = _adService.GetUserByUsername(ticket.RequestedFor);
                        if (requestedForUser != null)
                        {
                            ticket.RequestedForEmail = requestedForUser.Email;
                        }
                    }

                    // Get assigned user email from AD if changed
                    if (ticket.AssignedTo != existingTicket.AssignedTo && !string.IsNullOrWhiteSpace(ticket.AssignedTo))
                    {
                        var assignedUser = _adService.GetUserByUsername(ticket.AssignedTo);
                        if (assignedUser != null)
                        {
                            ticket.AssignedToEmail = assignedUser.Email;
                        }
                    }

                    // Update system fields
                    ticket.LastModifiedBy = currentUser;
                    ticket.LastModifiedDate = DateTime.Now;

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
                CreatedByEmail = adUser?.Email,
                CreatedDate = DateTime.Now
            };

            _context.TicketComments.Add(comment);

            // Update ticket modified info
            ticket.LastModifiedBy = currentUser;
            ticket.LastModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

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

        private async Task TrackChanges(Ticket oldTicket, Ticket newTicket, string changedBy)
        {
            if (oldTicket.Status != newTicket.Status)
            {
                await LogHistory(newTicket.TicketId, "Status", oldTicket.Status, newTicket.Status, changedBy);

                // Add system comment for status change
                var comment = new TicketComment
                {
                    TicketId = newTicket.TicketId,
                    CommentText = $"Status changed from '{oldTicket.Status}' to '{newTicket.Status}'.",
                    CommentType = "Status Change",
                    CreatedBy = changedBy,
                    CreatedDate = DateTime.Now
                };
                _context.TicketComments.Add(comment);
            }

            if (oldTicket.Priority != newTicket.Priority)
            {
                await LogHistory(newTicket.TicketId, "Priority", oldTicket.Priority, newTicket.Priority, changedBy);
            }

            if (oldTicket.Category != newTicket.Category)
            {
                await LogHistory(newTicket.TicketId, "Category", oldTicket.Category, newTicket.Category, changedBy);
            }

            if (oldTicket.AssignedTo != newTicket.AssignedTo)
            {
                await LogHistory(newTicket.TicketId, "AssignedTo", oldTicket.AssignedTo, newTicket.AssignedTo, changedBy);

                // Add system comment for assignment
                var comment = new TicketComment
                {
                    TicketId = newTicket.TicketId,
                    CommentText = $"Ticket assigned to '{newTicket.AssignedTo}'.",
                    CommentType = "Assignment",
                    CreatedBy = changedBy,
                    CreatedDate = DateTime.Now
                };
                _context.TicketComments.Add(comment);
            }

            if (oldTicket.Title != newTicket.Title)
            {
                await LogHistory(newTicket.TicketId, "Title", oldTicket.Title, newTicket.Title, changedBy);
            }
        }

        private async Task LogHistory(int ticketId, string fieldChanged, string? oldValue, string? newValue, string changedBy)
        {
            var history = new TicketHistory
            {
                TicketId = ticketId,
                FieldChanged = fieldChanged,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedBy = changedBy,
                ChangedDate = DateTime.Now
            };

            _context.TicketHistory.Add(history);
            await _context.SaveChangesAsync();
        }
    }
}
