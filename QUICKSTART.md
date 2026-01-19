# IT Help Desk - Quick Start Guide

This guide will help you get the IT Help Desk system up and running quickly.

## For Administrators

### 30-Minute Setup

#### 1. Database Setup (10 minutes)

Open SQL Server Management Studio and run these scripts in order:

```sql
-- Connect to your SQL Server instance
-- Run each script:
Database/01_CreateDatabase.sql
Database/02_CreateTables.sql
Database/03_CreateStoredProcedures.sql
```

#### 2. Configure Application (5 minutes)

Edit `ITHelpDesk.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ITHelpDesk;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "ActiveDirectory": {
    "Domain": "YOURDOMAIN",
    "LdapPath": "LDAP://yourdomain.com",
    "ITSupportGroup": "IT Support"
  }
}
```

Replace:
- `YOUR_SERVER` → Your SQL Server name
- `YOURDOMAIN` → Your domain name
- `LDAP://yourdomain.com` → Your LDAP path

#### 3. Build & Publish (5 minutes)

Open PowerShell in the ITHelpDesk.Web folder:

```powershell
# Build
dotnet build --configuration Release

# Publish to IIS folder
dotnet publish --configuration Release --output C:\inetpub\wwwroot\ITHelpDesk
```

#### 4. IIS Setup (10 minutes)

1. Open IIS Manager

2. Create Application Pool:
   - Name: `ITHelpDeskAppPool`
   - .NET CLR: `No Managed Code`
   - Click OK

3. Create Website:
   - Name: `IT Help Desk`
   - App Pool: `ITHelpDeskAppPool`
   - Path: `C:\inetpub\wwwroot\ITHelpDesk`
   - Port: `80` (or your choice)
   - Click OK

4. Enable Windows Authentication:
   - Select the website
   - Double-click **Authentication**
   - Enable: `Windows Authentication`
   - Disable: `Anonymous Authentication`

5. Start the website

#### 5. Test (5 minutes)

1. Browse to `http://localhost` (or your configured URL)
2. You should see the IT Help Desk login page
3. Try creating a test ticket
4. Verify AD lookups work in the "Assigned To" dropdown

Done! Your help desk is ready to use.

---

## For IT Staff

### Creating Your First Ticket

1. **Navigate** to the IT Help Desk website
2. Click **New Ticket** in the top navigation
3. Fill out the form:
   - **Title**: "Printer not working in Room 205"
   - **Description**: Detailed information about the issue
   - **Priority**: Choose based on urgency
   - **Category**: Select the appropriate category
   - **Assigned To**: Pick an IT staff member (optional)
   - **Due Date**: Set if needed (optional)
4. Click **Create Ticket**

### Managing Tickets

**View All Tickets**
- Homepage shows all tickets
- Use filters to narrow down results
- Click on any ticket to view details

**Update a Ticket**
- Open the ticket
- Click **Edit Ticket**
- Change status, priority, or assignment
- Click **Save Changes**

**Add Comments**
- Open the ticket details
- Scroll to "Comments & Updates"
- Type your comment
- Click **Add Comment**

### Understanding Ticket Status

- **New**: Just created, not yet addressed
- **In Progress**: Someone is working on it
- **Resolved**: Issue is fixed, awaiting confirmation
- **Closed**: Ticket is complete

### Priority Levels

- **Critical**: System down, major outage (fix immediately)
- **High**: Significantly impacting work (fix today)
- **Medium**: Normal priority (fix this week)
- **Low**: Minor issue (fix when available)

### Categories

- **Hardware**: Physical equipment issues
- **Software**: Application problems
- **Network**: Connectivity issues
- **Access/Permissions**: Login, permissions, access requests
- **Other**: Everything else

### Searching Tickets

Use the search box to find tickets by:
- Ticket title
- Description content
- Comments

Use filters to narrow by:
- Status
- Priority
- Category
- Assigned person

### Viewing History

**Your Tickets**
- Click **My History** in navigation
- See all tickets you created or are assigned to

**Ticket History**
- Each ticket shows a complete change history
- See who changed what and when

---

## Common Tasks

### Assign a Ticket to Yourself

1. Open the ticket
2. Click **Edit Ticket**
3. Select your name in **Assigned To**
4. Click **Save Changes**

### Mark a Ticket as Resolved

1. Open the ticket
2. Click **Edit Ticket**
3. Change **Status** to "Resolved"
4. Add a comment explaining the resolution
5. Click **Save Changes**

### Close a Ticket

1. Verify the issue is completely resolved
2. Open the ticket
3. Click **Edit Ticket**
4. Change **Status** to "Closed"
5. Click **Save Changes**

### Find Overdue Tickets

Overdue tickets are highlighted in red on the main ticket list.

### Search for a Specific User's Tickets

1. Go to the main tickets page
2. Use search: type the username
3. Or click **My History** to see your own tickets

---

## Tips for Good Ticket Management

### Writing Ticket Titles
✅ **Good**: "Outlook crashes when opening attachments"
❌ **Bad**: "Outlook problem"

### Writing Descriptions
Include:
- What happened
- When it started
- What you've already tried
- Any error messages (exact text)
- Who/what is affected

Example:
```
User Jane Smith reports that Excel crashes when opening
large spreadsheets. Started this morning after Windows
updates. Tried restarting computer - issue persists.
Error message: "Excel has stopped working."
Affects: Jane Smith, Accounting Department
```

### Using Priorities Effectively
- Don't mark everything as Critical
- Be realistic about impact
- Consider: How many users? Can they work? Any workarounds?

### Adding Useful Comments
Document:
- What you tried
- What worked/didn't work
- Next steps
- Time spent (optional)

### Closing Tickets
Before closing:
- Verify the issue is actually resolved
- Get user confirmation if possible
- Document the solution
- Update any relevant documentation

---

## Need Help?

**Can't login?**
- Make sure you're on the domain
- Try a different browser
- Contact your IT administrator

**Don't see IT staff in dropdowns?**
- Verify you're a member of the "IT Support" AD group
- Contact your IT administrator

**Something not working?**
- Check with your IT administrator
- Provide specific details about the error
- Include screenshots if helpful

---

## Keyboard Shortcuts

- `Ctrl + /`: Focus search box (when implemented)
- `Ctrl + N`: New ticket (browser dependent)

---

## Best Practices

1. **Create tickets promptly** - Don't let issues pile up
2. **Update status regularly** - Keep tickets current
3. **Use comments liberally** - Document everything
4. **Close completed tickets** - Keep the list clean
5. **Search before creating** - Check for duplicates
6. **Be specific** - Good details = faster resolution
7. **Follow up** - Check that resolved tickets stay resolved
8. **Review history** - Learn from past solutions

---

## Dashboard Overview

The main dashboard shows:

- **New Tickets**: Tickets that need attention
- **In Progress**: Tickets being worked on
- **High Priority**: Urgent tickets that need focus
- **Overdue**: Tickets past their due date

Use these statistics to:
- Prioritize your work
- Identify bottlenecks
- Track team workload

---

## Quick Reference Card

| Task | Steps |
|------|-------|
| Create ticket | New Ticket → Fill form → Create |
| Assign ticket | Edit → Choose assignee → Save |
| Update status | Edit → Change status → Save |
| Add comment | Open ticket → Type comment → Add Comment |
| Search | Use search box on main page |
| View history | Click "My History" in navigation |
| Close ticket | Edit → Status: Closed → Save |

---

For detailed documentation, see **README.md**

For deployment instructions, see **DEPLOYMENT_CHECKLIST.md**
