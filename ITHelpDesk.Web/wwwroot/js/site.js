// IT Help Desk Ticket System - JavaScript

// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function() {
    const alerts = document.querySelectorAll('.alert:not(.alert-info):not(.alert-warning)');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});

// Confirmation dialogs for delete actions
function confirmDelete(message) {
    return confirm(message || 'Are you sure you want to delete this item? This action cannot be undone.');
}

// Format date inputs to local timezone
document.addEventListener('DOMContentLoaded', function() {
    const dateInputs = document.querySelectorAll('input[type="date"]');
    dateInputs.forEach(function(input) {
        if (!input.value) {
            // Set min date to today for due date fields
            const today = new Date().toISOString().split('T')[0];
            input.setAttribute('min', today);
        }
    });
});

// Highlight search terms in results
function highlightSearchTerms(searchTerm) {
    if (!searchTerm) return;

    const regex = new RegExp(`(${searchTerm})`, 'gi');
    const elements = document.querySelectorAll('td, p');

    elements.forEach(function(element) {
        if (element.children.length === 0) {
            const text = element.textContent;
            if (regex.test(text)) {
                element.innerHTML = text.replace(regex, '<mark>$1</mark>');
            }
        }
    });
}

// Auto-resize textareas
document.addEventListener('DOMContentLoaded', function() {
    const textareas = document.querySelectorAll('textarea');
    textareas.forEach(function(textarea) {
        textarea.addEventListener('input', function() {
            this.style.height = 'auto';
            this.style.height = (this.scrollHeight) + 'px';
        });
    });
});

// Print ticket details
function printTicket() {
    window.print();
}

// Export ticket to text
function exportTicketToText(ticketId, title, description) {
    const text = `Ticket #${ticketId}\n\nTitle: ${title}\n\nDescription:\n${description}`;
    const blob = new Blob([text], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Ticket_${ticketId}.txt`;
    a.click();
    window.URL.revokeObjectURL(url);
}
