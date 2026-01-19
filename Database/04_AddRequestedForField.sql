-- Add RequestedFor field to Tickets table
USE ITHelpDesk;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND name = 'RequestedFor')
BEGIN
    ALTER TABLE [dbo].[Tickets]
    ADD [RequestedFor] NVARCHAR(255) NULL,
        [RequestedForEmail] NVARCHAR(255) NULL;

    PRINT 'Added RequestedFor and RequestedForEmail columns to Tickets table.';
END
ELSE
BEGIN
    PRINT 'RequestedFor column already exists.';
END
GO

-- Create index for RequestedFor
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tickets_RequestedFor' AND object_id = OBJECT_ID('dbo.Tickets'))
BEGIN
    CREATE INDEX IX_Tickets_RequestedFor ON [dbo].[Tickets]([RequestedFor]);
    PRINT 'Index IX_Tickets_RequestedFor created successfully.';
END
GO
