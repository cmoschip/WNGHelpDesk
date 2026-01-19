# IT Help Desk - Deployment Checklist

Use this checklist to ensure a smooth deployment of the IT Help Desk Ticket Tracking System.

## Pre-Deployment

### Server Requirements
- [ ] Windows Server is available and accessible
- [ ] IIS is installed and configured
- [ ] SQL Server 2019 (or later) is installed
- [ ] .NET 8.0 Runtime (Hosting Bundle) is installed
- [ ] Server is joined to Active Directory domain
- [ ] You have administrator access to the server
- [ ] You have SQL Server sysadmin or dbcreator rights

### Active Directory Setup
- [ ] IT Support security group exists in AD
- [ ] IT staff members are added to the IT Support group
- [ ] You have documented:
  - Domain name: ___________________
  - LDAP path: ___________________
  - IT Support group name: ___________________

### Network & Security
- [ ] Firewall rules allow web traffic (port 80/443)
- [ ] SSL certificate is available (if using HTTPS)
- [ ] SQL Server port (1433) is accessible from web server
- [ ] Backup strategy is defined

## Database Deployment

### SQL Server Setup
- [ ] Connected to SQL Server using SSMS
- [ ] Executed `01_CreateDatabase.sql`
- [ ] Executed `02_CreateTables.sql`
- [ ] Executed `03_CreateStoredProcedures.sql`
- [ ] Verified database `ITHelpDesk` exists
- [ ] Verified all tables exist:
  - [ ] Tickets
  - [ ] TicketComments
  - [ ] TicketHistory
- [ ] Verified stored procedures exist:
  - [ ] SearchTickets
  - [ ] GetTicketsByUser
  - [ ] GetTicketWithDetails
  - [ ] GetDashboardStats

### Database Permissions
- [ ] Created SQL login for application pool identity
- [ ] Granted db_datareader role
- [ ] Granted db_datawriter role
- [ ] Tested connection from application

## Application Configuration

### Configuration Files
- [ ] Updated `appsettings.json`:
  - [ ] SQL Server connection string updated
  - [ ] Domain name configured
  - [ ] LDAP path configured
  - [ ] IT Support group name configured
- [ ] Reviewed `web.config` settings
- [ ] Verified Windows Authentication is enabled in web.config

### Build and Publish
- [ ] Opened command prompt in project directory
- [ ] Executed `dotnet build --configuration Release`
- [ ] Build completed successfully (no errors)
- [ ] Executed `dotnet publish --configuration Release --output [PublishPath]`
- [ ] Publish completed successfully
- [ ] Verified all files copied to publish directory:
  - [ ] ITHelpDesk.Web.dll
  - [ ] web.config
  - [ ] appsettings.json
  - [ ] wwwroot folder with CSS/JS
  - [ ] Views folder

## IIS Configuration

### Application Pool
- [ ] Created new Application Pool: `ITHelpDeskAppPool`
- [ ] Set .NET CLR version to: `No Managed Code`
- [ ] Set Managed pipeline mode to: `Integrated`
- [ ] Configured identity:
  - [ ] Using ApplicationPoolIdentity, OR
  - [ ] Using custom domain account with proper permissions
- [ ] Application Pool is started

### Website Setup
- [ ] Created new website: `IT Help Desk`
- [ ] Assigned Application Pool: `ITHelpDeskAppPool`
- [ ] Set physical path to published application
- [ ] Configured bindings:
  - [ ] Protocol: HTTP or HTTPS
  - [ ] Port: ___________________
  - [ ] Host name (if applicable): ___________________
  - [ ] SSL certificate (if HTTPS): ___________________
- [ ] Website is started

### Authentication
- [ ] Opened Authentication settings for website
- [ ] Enabled Windows Authentication
- [ ] Disabled Anonymous Authentication
- [ ] Verified Windows Authentication providers are configured
- [ ] Tested authentication by browsing to site

### Permissions
- [ ] Verified IIS_IUSRS has read access to application folder
- [ ] Verified Application Pool identity has read access
- [ ] Verified Application Pool identity has write access to logs folder
- [ ] Verified Application Pool identity can query Active Directory

## Testing

### Basic Functionality
- [ ] Browsed to website URL: ___________________
- [ ] Automatically authenticated (no credential prompt)
- [ ] Redirected to Tickets page successfully
- [ ] Dashboard statistics display correctly (even if zero)

### Active Directory Integration
- [ ] Created a new test ticket
- [ ] "Assigned To" dropdown populates with IT staff members
- [ ] User information displays correctly
- [ ] Email addresses populate from AD

### Database Operations
- [ ] Successfully created a test ticket
- [ ] Ticket appears in the list
- [ ] Clicked on ticket to view details
- [ ] Added a comment to the ticket
- [ ] Comment appears in ticket history
- [ ] Edited the ticket (changed status/priority)
- [ ] Changes reflected in ticket history
- [ ] Search functionality works

### User Experience
- [ ] All pages load without errors
- [ ] Navigation menu works correctly
- [ ] Filters and search function properly
- [ ] Forms validate correctly
- [ ] Bootstrap styling displays properly
- [ ] Icons display (Bootstrap Icons)

### Performance
- [ ] Page load times are acceptable
- [ ] No timeout errors
- [ ] SQL queries execute quickly

## Post-Deployment

### Documentation
- [ ] Documented server details:
  - Server name: ___________________
  - Website URL: ___________________
  - SQL Server name: ___________________
- [ ] Created administrator guide for IT staff
- [ ] Created user guide for ticket creation (if needed)
- [ ] Documented backup procedures
- [ ] Documented maintenance schedule

### Monitoring
- [ ] Set up database backups:
  - [ ] Full backup schedule: ___________________
  - [ ] Transaction log backup schedule: ___________________
  - [ ] Backup retention policy: ___________________
- [ ] Configured IIS log retention
- [ ] Set up monitoring/alerts (optional):
  - [ ] Website availability monitoring
  - [ ] Disk space monitoring
  - [ ] SQL Server monitoring

### Training
- [ ] Trained IT staff on system usage
- [ ] Demonstrated ticket creation process
- [ ] Showed how to search and filter tickets
- [ ] Explained status workflow
- [ ] Covered how to add comments
- [ ] Reviewed reporting/statistics

### Cleanup
- [ ] Deleted test tickets (if desired)
- [ ] Verified no sensitive test data remains
- [ ] Removed any temporary test accounts
- [ ] Cleaned up old files from development

## Troubleshooting Reference

If issues occur during deployment, refer to:
- README.md - Troubleshooting section
- IIS logs: `C:\inetpub\logs\LogFiles\`
- Application stdout logs: `[PublishPath]\logs\stdout`
- Windows Event Viewer - Application logs
- SQL Server error logs

## Sign-Off

- [ ] Deployment completed successfully
- [ ] All tests passed
- [ ] Documentation complete
- [ ] IT staff trained
- [ ] System ready for production use

**Deployed by**: ___________________
**Date**: ___________________
**Time**: ___________________

**Notes/Issues**:
_______________________________________________________________________________
_______________________________________________________________________________
_______________________________________________________________________________
