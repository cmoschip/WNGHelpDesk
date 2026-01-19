-- =============================================
-- Create Tables for IT Help Desk System
-- =============================================

USE ITHelpDesk;
GO

-- =============================================
-- Table: Tickets
-- Stores all help desk tickets
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Tickets] (
        [TicketId] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(255) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'New', -- New, In Progress, Resolved, Closed
        [Priority] NVARCHAR(50) NOT NULL DEFAULT 'Medium', -- Low, Medium, High, Critical
        [Category] NVARCHAR(100) NULL, -- Hardware, Software, Network, Access/Permissions, Other
        [CreatedBy] NVARCHAR(255) NOT NULL, -- AD Username (domain\username)
        [CreatedByEmail] NVARCHAR(255) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [AssignedTo] NVARCHAR(255) NULL, -- AD Username (domain\username)
        [AssignedToEmail] NVARCHAR(255) NULL,
        [DueDate] DATETIME2 NULL,
        [ResolvedDate] DATETIME2 NULL,
        [ClosedDate] DATETIME2 NULL,
        [LastModifiedBy] NVARCHAR(255) NULL,
        [LastModifiedDate] DATETIME2 NULL
    );

    PRINT 'Table Tickets created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Tickets already exists.';
END
GO

-- =============================================
-- Table: TicketComments
-- Stores all comments/updates for tickets
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TicketComments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TicketComments] (
        [CommentId] INT IDENTITY(1,1) PRIMARY KEY,
        [TicketId] INT NOT NULL,
        [CommentText] NVARCHAR(MAX) NOT NULL,
        [CommentType] NVARCHAR(50) NOT NULL DEFAULT 'Comment', -- Comment, Status Change, Assignment, Resolution
        [CreatedBy] NVARCHAR(255) NOT NULL,
        [CreatedByEmail] NVARCHAR(255) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [IsInternal] BIT NOT NULL DEFAULT 0, -- For future use if you want internal-only notes
        CONSTRAINT FK_TicketComments_Tickets FOREIGN KEY (TicketId)
            REFERENCES [dbo].[Tickets](TicketId) ON DELETE CASCADE
    );

    PRINT 'Table TicketComments created successfully.';
END
ELSE
BEGIN
    PRINT 'Table TicketComments already exists.';
END
GO

-- =============================================
-- Table: TicketHistory
-- Stores audit trail of all ticket changes
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TicketHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TicketHistory] (
        [HistoryId] INT IDENTITY(1,1) PRIMARY KEY,
        [TicketId] INT NOT NULL,
        [FieldChanged] NVARCHAR(100) NOT NULL, -- Status, Priority, AssignedTo, etc.
        [OldValue] NVARCHAR(MAX) NULL,
        [NewValue] NVARCHAR(MAX) NULL,
        [ChangedBy] NVARCHAR(255) NOT NULL,
        [ChangedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TicketHistory_Tickets FOREIGN KEY (TicketId)
            REFERENCES [dbo].[Tickets](TicketId) ON DELETE CASCADE
    );

    PRINT 'Table TicketHistory created successfully.';
END
ELSE
BEGIN
    PRINT 'Table TicketHistory already exists.';
END
GO

-- =============================================
-- Create Indexes for Performance
-- =============================================

-- Index on Tickets.Status for filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tickets_Status' AND object_id = OBJECT_ID('dbo.Tickets'))
BEGIN
    CREATE INDEX IX_Tickets_Status ON [dbo].[Tickets]([Status]);
    PRINT 'Index IX_Tickets_Status created successfully.';
END

-- Index on Tickets.AssignedTo for filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tickets_AssignedTo' AND object_id = OBJECT_ID('dbo.Tickets'))
BEGIN
    CREATE INDEX IX_Tickets_AssignedTo ON [dbo].[Tickets]([AssignedTo]);
    PRINT 'Index IX_Tickets_AssignedTo created successfully.';
END

-- Index on Tickets.CreatedBy for user history
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tickets_CreatedBy' AND object_id = OBJECT_ID('dbo.Tickets'))
BEGIN
    CREATE INDEX IX_Tickets_CreatedBy ON [dbo].[Tickets]([CreatedBy]);
    PRINT 'Index IX_Tickets_CreatedBy created successfully.';
END

-- Index on Tickets.CreatedDate for sorting
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tickets_CreatedDate' AND object_id = OBJECT_ID('dbo.Tickets'))
BEGIN
    CREATE INDEX IX_Tickets_CreatedDate ON [dbo].[Tickets]([CreatedDate] DESC);
    PRINT 'Index IX_Tickets_CreatedDate created successfully.';
END

-- Index on TicketComments.TicketId for lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TicketComments_TicketId' AND object_id = OBJECT_ID('dbo.TicketComments'))
BEGIN
    CREATE INDEX IX_TicketComments_TicketId ON [dbo].[TicketComments]([TicketId]);
    PRINT 'Index IX_TicketComments_TicketId created successfully.';
END

-- Index on TicketHistory.TicketId for lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TicketHistory_TicketId' AND object_id = OBJECT_ID('dbo.TicketHistory'))
BEGIN
    CREATE INDEX IX_TicketHistory_TicketId ON [dbo].[TicketHistory]([TicketId]);
    PRINT 'Index IX_TicketHistory_TicketId created successfully.';
END

GO

PRINT 'All tables and indexes created successfully.';
GO
