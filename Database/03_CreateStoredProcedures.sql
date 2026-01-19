-- =============================================
-- Stored Procedures for IT Help Desk System
-- =============================================

USE ITHelpDesk;
GO

-- =============================================
-- Stored Procedure: SearchTickets
-- Search tickets by keyword in title, description, or comments
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SearchTickets]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SearchTickets];
GO

CREATE PROCEDURE [dbo].[SearchTickets]
    @SearchTerm NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT t.*
    FROM [dbo].[Tickets] t
    LEFT JOIN [dbo].[TicketComments] tc ON t.TicketId = tc.TicketId
    WHERE
        t.Title LIKE '%' + @SearchTerm + '%'
        OR t.Description LIKE '%' + @SearchTerm + '%'
        OR tc.CommentText LIKE '%' + @SearchTerm + '%'
    ORDER BY t.CreatedDate DESC;
END
GO

PRINT 'Stored Procedure SearchTickets created successfully.';
GO

-- =============================================
-- Stored Procedure: GetTicketsByUser
-- Get all tickets created by or assigned to a specific user
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTicketsByUser]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetTicketsByUser];
GO

CREATE PROCEDURE [dbo].[GetTicketsByUser]
    @Username NVARCHAR(255),
    @IncludeClosed BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @IncludeClosed = 1
    BEGIN
        SELECT *
        FROM [dbo].[Tickets]
        WHERE CreatedBy = @Username OR AssignedTo = @Username
        ORDER BY CreatedDate DESC;
    END
    ELSE
    BEGIN
        SELECT *
        FROM [dbo].[Tickets]
        WHERE (CreatedBy = @Username OR AssignedTo = @Username)
            AND Status NOT IN ('Closed')
        ORDER BY CreatedDate DESC;
    END
END
GO

PRINT 'Stored Procedure GetTicketsByUser created successfully.';
GO

-- =============================================
-- Stored Procedure: GetTicketWithDetails
-- Get ticket with all comments and history
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTicketWithDetails]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetTicketWithDetails];
GO

CREATE PROCEDURE [dbo].[GetTicketWithDetails]
    @TicketId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Get ticket details
    SELECT * FROM [dbo].[Tickets] WHERE TicketId = @TicketId;

    -- Get all comments for the ticket
    SELECT * FROM [dbo].[TicketComments]
    WHERE TicketId = @TicketId
    ORDER BY CreatedDate ASC;

    -- Get all history for the ticket
    SELECT * FROM [dbo].[TicketHistory]
    WHERE TicketId = @TicketId
    ORDER BY ChangedDate ASC;
END
GO

PRINT 'Stored Procedure GetTicketWithDetails created successfully.';
GO

-- =============================================
-- Stored Procedure: GetDashboardStats
-- Get statistics for dashboard
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDashboardStats]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetDashboardStats];
GO

CREATE PROCEDURE [dbo].[GetDashboardStats]
AS
BEGIN
    SET NOCOUNT ON;

    -- Overall stats
    SELECT
        COUNT(*) as TotalTickets,
        SUM(CASE WHEN Status = 'New' THEN 1 ELSE 0 END) as NewTickets,
        SUM(CASE WHEN Status = 'In Progress' THEN 1 ELSE 0 END) as InProgressTickets,
        SUM(CASE WHEN Status = 'Resolved' THEN 1 ELSE 0 END) as ResolvedTickets,
        SUM(CASE WHEN Status = 'Closed' THEN 1 ELSE 0 END) as ClosedTickets,
        SUM(CASE WHEN Priority = 'Critical' THEN 1 ELSE 0 END) as CriticalTickets,
        SUM(CASE WHEN Priority = 'High' THEN 1 ELSE 0 END) as HighPriorityTickets,
        SUM(CASE WHEN DueDate IS NOT NULL AND DueDate < GETDATE() AND Status NOT IN ('Closed', 'Resolved') THEN 1 ELSE 0 END) as OverdueTickets
    FROM [dbo].[Tickets];

    -- Tickets by category
    SELECT
        Category,
        COUNT(*) as Count
    FROM [dbo].[Tickets]
    WHERE Status NOT IN ('Closed')
    GROUP BY Category
    ORDER BY Count DESC;
END
GO

PRINT 'Stored Procedure GetDashboardStats created successfully.';
GO

PRINT 'All stored procedures created successfully.';
GO
