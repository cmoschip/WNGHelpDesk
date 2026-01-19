# IT Help Desk Ticket Tracking System

A lightweight, home-grown IT help desk ticket tracking system built with ASP.NET Core MVC, SQL Server, and Active Directory integration. Designed for small IT departments (2-3 staff members) with minimal administrative overhead.

## Features

- **Active Directory Integration**: Automatic user lookup for usernames, emails, phone numbers, and group memberships
- **Ticket Management**: Create, edit, view, and track IT support tickets
- **Status Workflow**: New → In Progress → Resolved → Closed
- **Priority Levels**: Low, Medium, High, Critical
- **Categories**: Hardware, Software, Network, Access/Permissions, Other
- **Comments & History**: Full audit trail of all ticket changes and comments
- **Search Functionality**: Search across ticket titles, descriptions, and comments
- **User History**: View all tickets created by or assigned to a specific user
- **Dashboard Statistics**: Quick overview of ticket counts by status and priority
- **Overdue Tracking**: Automatic highlighting of overdue tickets
- **Windows Authentication**: Seamless integration with existing Windows infrastructure

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server 2019 (or later)
- **Web Server**: IIS (Internet Information Services)
- **Authentication**: Windows Authentication with Active Directory
- **Frontend**: Bootstrap 5.3, Bootstrap Icons
- **ORM**: Entity Framework Core 8.0

## Prerequisites

Before deploying the IT Help Desk system, ensure you have:

1. **Windows Server** with IIS installed
2. **SQL Server 2019** (or later) installed and accessible
3. **.NET 8.0 Runtime** installed on the web server
4. **Active Directory** domain environment
5. **IIS Windows Authentication** module enabled
6. Appropriate permissions to:
   - Create databases on SQL Server
   - Configure IIS websites
   - Query Active Directory
   - Create AD security groups

## Installation Guide

### Step 1: Database Setup

1. Connect to your SQL Server using SQL Server Management Studio (SSMS)

2. Run the database scripts in order:
   ```
   Database/01_CreateDatabase.sql
   Database/02_CreateTables.sql
   Database/03_CreateStoredProcedures.sql
   ```

3. Verify the database was created successfully:
   ```sql
   USE ITHelpDesk;
   SELECT * FROM INFORMATION_SCHEMA.TABLES;
   ```

### Step 2: Active Directory Configuration

1. Create an AD security group for IT support staff (if not exists):
   - Open **Active Directory Users and Computers**
   - Create a new group: `IT Support` (or your preferred name)
   - Add your IT staff members to this group

2. Note your domain information:
   - Domain name (e.g., `CONTOSO`)
   - LDAP path (e.g., `LDAP://contoso.com`)

### Step 3: Application Configuration

1. Navigate to the application directory:
   ```
   ITHelpDesk.Web/
   ```

2. Open `appsettings.json` and update the following settings:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=ITHelpDesk;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
     },
     "ActiveDirectory": {
       "Domain": "YOUR_DOMAIN",
       "LdapPath": "LDAP://YOUR_DOMAIN",
       "ITSupportGroup": "IT Support"
     }
   }
   ```

   **Replace:**
   - `YOUR_SQL_SERVER` with your SQL Server name (e.g., `SERVER01` or `localhost`)
   - `YOUR_DOMAIN` with your domain name (e.g., `CONTOSO`)
   - Update the `ITSupportGroup` if you used a different group name

### Step 4: Build the Application

1. Open a command prompt in the project directory

2. Build the application:
   ```powershell
   cd ITHelpDesk.Web
   dotnet build --configuration Release
   ```

3. Publish the application:
   ```powershell
   dotnet publish --configuration Release --output C:\inetpub\wwwroot\ITHelpDesk
   ```

### Step 5: IIS Configuration

1. Open **Internet Information Services (IIS) Manager**

2. Create a new Application Pool:
   - Right-click **Application Pools** → **Add Application Pool**
   - Name: `ITHelpDeskAppPool`
   - .NET CLR version: `No Managed Code`
   - Managed pipeline mode: `Integrated`
   - Click **OK**

3. Configure Application Pool Identity:
   - Right-click `ITHelpDeskAppPool` → **Advanced Settings**
   - Set **Identity** to use an account with:
     - SQL Server access
     - Active Directory read permissions
   - Or use `ApplicationPoolIdentity` and grant permissions accordingly

4. Create a new Website:
   - Right-click **Sites** → **Add Website**
   - Site name: `IT Help Desk`
   - Application pool: Select `ITHelpDeskAppPool`
   - Physical path: `C:\inetpub\wwwroot\ITHelpDesk`
   - Binding:
     - Type: `https` (recommended)
     - Port: `443` (or your preferred port)
     - Or use `http` port `80` for testing
   - Click **OK**

5. Enable Windows Authentication:
   - Select your website in IIS Manager
   - Double-click **Authentication**
   - **Windows Authentication**: Enable
   - **Anonymous Authentication**: Disable

6. Configure Application Settings:
   - Select your website
   - Double-click **Configuration Editor**
   - Section: `system.webServer/security/authentication/windowsAuthentication`
   - Set `enabled` to `True`
   - Set `useKernelMode` to `True` (recommended for performance)

### Step 6: Permissions Setup

1. **SQL Server Permissions**:

   If using ApplicationPoolIdentity:
   ```sql
   USE ITHelpDesk;
   CREATE USER [IIS APPPOOL\ITHelpDeskAppPool] FOR LOGIN [IIS APPPOOL\ITHelpDeskAppPool];
   EXEC sp_addrolemember 'db_datareader', [IIS APPPOOL\ITHelpDeskAppPool];
   EXEC sp_addrolemember 'db_datawriter', [IIS APPPOOL\ITHelpDeskAppPool];
   ```

2. **File System Permissions**:
   - Grant the Application Pool identity read/write access to:
     - Application directory: `C:\inetpub\wwwroot\ITHelpDesk`
     - Logs directory (will be created automatically)

3. **Active Directory Permissions**:
   - The Application Pool identity needs:
     - Read access to query user information
     - Read access to query group memberships
   - Usually granted by default for domain-joined servers

### Step 7: Verify Installation

1. Browse to your website (e.g., `http://localhost` or `https://yourdomain/helpdesk`)

2. You should be automatically authenticated via Windows Authentication

3. The application should redirect to the Tickets page

4. Try creating a test ticket to verify:
   - Database connectivity
   - Active Directory lookups (in the "Assigned To" dropdown)
   - Comment functionality

## Configuration Options

### appsettings.json Settings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "SQL Server connection string"
  },
  "ActiveDirectory": {
    "Domain": "Your AD domain name",
    "LdapPath": "LDAP path to your domain",
    "ITSupportGroup": "Name of IT support security group"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Customizing Categories

To modify ticket categories, edit the dropdown options in:
- `Views/Tickets/Create.cshtml`
- `Views/Tickets/Edit.cshtml`
- `Views/Tickets/Index.cshtml` (filter dropdown)

### Customizing Status Workflow

The default workflow is: **New → In Progress → Resolved → Closed**

To modify, update the status options in:
- `Views/Tickets/Edit.cshtml`
- `Views/Tickets/Index.cshtml`
- `Models/Ticket.cs` (if changing defaults)

## Usage Guide

### Creating a Ticket

1. Click **New Ticket** in the navigation bar
2. Fill in the ticket details:
   - **Title**: Brief description
   - **Description**: Detailed information about the issue
   - **Priority**: Select urgency level
   - **Category**: Choose issue type
   - **Assigned To**: Select IT staff member (optional)
   - **Due Date**: Set deadline (optional)
3. Click **Create Ticket**

### Managing Tickets

- **View All Tickets**: Main dashboard shows all tickets with filters
- **Search**: Use the search box to find specific tickets
- **Filter**: Filter by status, priority, category, or assigned user
- **View Details**: Click on a ticket ID or title
- **Add Comments**: Use the comment box on the ticket details page
- **Update Status**: Edit the ticket to change status, priority, or assignment

### Viewing History

- **My History**: View all tickets you created or are assigned to
- **Change History**: Each ticket shows a complete audit trail
- **Comments**: All updates and comments are preserved chronologically

## Maintenance

### Database Backups

Schedule regular backups of the `ITHelpDesk` database:

```sql
BACKUP DATABASE ITHelpDesk
TO DISK = 'C:\Backups\ITHelpDesk_Full.bak'
WITH FORMAT, COMPRESSION;
```

### Log Management

Application logs are stored in:
- IIS logs: `C:\inetpub\logs\LogFiles\`
- Application logs: Configured in `appsettings.json`

### Performance Optimization

For better performance:
1. Regularly update statistics:
   ```sql
   USE ITHelpDesk;
   EXEC sp_updatestats;
   ```

2. Rebuild indexes periodically:
   ```sql
   USE ITHelpDesk;
   ALTER INDEX ALL ON Tickets REBUILD;
   ALTER INDEX ALL ON TicketComments REBUILD;
   ALTER INDEX ALL ON TicketHistory REBUILD;
   ```

### Archiving Old Tickets

To keep the database lean, consider archiving closed tickets older than 1-2 years:

```sql
-- Example: Archive tickets closed more than 2 years ago
SELECT * INTO ITHelpDesk_Archive.dbo.Tickets_Archived
FROM ITHelpDesk.dbo.Tickets
WHERE Status = 'Closed'
  AND ClosedDate < DATEADD(YEAR, -2, GETDATE());
```

## Troubleshooting

### Cannot Connect to SQL Server

**Error**: "Cannot open database ITHelpDesk"

**Solutions**:
1. Verify SQL Server is running
2. Check connection string in `appsettings.json`
3. Verify Application Pool identity has SQL Server access
4. Test connection using SQL Server Management Studio

### Windows Authentication Not Working

**Error**: Prompted for credentials or access denied

**Solutions**:
1. Verify Windows Authentication is enabled in IIS
2. Verify Anonymous Authentication is disabled
3. Check browser settings (Internet Explorer/Edge may require adding site to Local Intranet)
4. For Chrome/Firefox, may need to configure Windows Authentication support

### Active Directory Lookups Failing

**Error**: Dropdowns empty or AD users not found

**Solutions**:
1. Verify domain name and LDAP path in `appsettings.json`
2. Verify Application Pool identity has AD read permissions
3. Check that the server is domain-joined
4. Test AD connectivity using PowerShell:
   ```powershell
   Get-ADUser -Filter * -SearchBase "DC=yourdomain,DC=com"
   ```

### Page Not Found (404) Errors

**Solutions**:
1. Verify .NET 8.0 Runtime is installed
2. Check IIS Application Pool is running
3. Verify web.config is present
4. Check IIS logs for detailed errors

### Internal Server Error (500)

**Solutions**:
1. Enable detailed errors in `web.config`:
   ```xml
   <aspNetCore ... stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" />
   ```
2. Check stdout logs in the logs folder
3. Verify all NuGet packages are restored
4. Check Event Viewer for application errors

## Security Considerations

1. **Always use HTTPS** in production
2. **Restrict access** to IT staff only via AD group membership
3. **Regular backups** of the database
4. **Keep .NET runtime updated** with security patches
5. **Monitor access logs** for unauthorized access attempts
6. **Secure SQL Server** with proper firewall rules
7. **Use strong SQL Server authentication** if not using Windows Auth

## Future Enhancements

Potential features to add:

- Email notifications on ticket creation/updates
- File attachments for tickets
- SLA (Service Level Agreement) tracking
- Reporting and analytics dashboard
- Mobile-responsive improvements
- Integration with monitoring tools
- Knowledge base articles
- Ticket templates for common issues
- Time tracking for tickets

## Support

For issues or questions about this system:
1. Check the troubleshooting section above
2. Review IIS and application logs
3. Verify all configuration settings
4. Check SQL Server connectivity and permissions

## License

This is a custom in-house application developed for internal use.

## Version History

- **Version 1.0** (Initial Release)
  - Core ticket management functionality
  - Active Directory integration
  - Search and filtering
  - Comments and history tracking
  - Dashboard statistics
