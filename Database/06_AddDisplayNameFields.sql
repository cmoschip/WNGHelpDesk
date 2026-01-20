-- Add display name fields to support showing full names instead of usernames
USE [ITHelpDesk]
GO

-- Add display name fields to Tickets table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND name = 'CreatedByName')
BEGIN
    ALTER TABLE [dbo].[Tickets] ADD [CreatedByName] NVARCHAR(255) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND name = 'RequestedForName')
BEGIN
    ALTER TABLE [dbo].[Tickets] ADD [RequestedForName] NVARCHAR(255) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND name = 'AssignedToName')
BEGIN
    ALTER TABLE [dbo].[Tickets] ADD [AssignedToName] NVARCHAR(255) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND name = 'LastModifiedByName')
BEGIN
    ALTER TABLE [dbo].[Tickets] ADD [LastModifiedByName] NVARCHAR(255) NULL;
END
GO

-- Add display name field to TicketComments table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TicketComments]') AND name = 'CreatedByName')
BEGIN
    ALTER TABLE [dbo].[TicketComments] ADD [CreatedByName] NVARCHAR(255) NULL;
END
GO

-- Add display name field to TicketHistory table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TicketHistory]') AND name = 'ChangedByName')
BEGIN
    ALTER TABLE [dbo].[TicketHistory] ADD [ChangedByName] NVARCHAR(255) NULL;
END
GO

-- Add display name field to TicketAttachments table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TicketAttachments]') AND name = 'UploadedByName')
BEGIN
    ALTER TABLE [dbo].[TicketAttachments] ADD [UploadedByName] NVARCHAR(255) NULL;
END
GO

PRINT 'Database migration 06 completed successfully - Display name fields added.'
GO
