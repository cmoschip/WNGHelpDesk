-- Add ComputerName field to Tickets table
USE [ITHelpDesk]
GO

-- Add ComputerName column to Tickets table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tickets]') AND name = 'ComputerName')
BEGIN
    ALTER TABLE [dbo].[Tickets]
    ADD [ComputerName] NVARCHAR(255) NULL;

    CREATE INDEX IX_Tickets_ComputerName ON [dbo].[Tickets]([ComputerName]);
END
GO

-- Create TicketAttachments table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TicketAttachments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TicketAttachments] (
        [TicketAttachmentId] INT IDENTITY(1,1) NOT NULL,
        [TicketId] INT NOT NULL,
        [FileName] NVARCHAR(255) NOT NULL,
        [FilePath] NVARCHAR(500) NOT NULL,
        [FileSize] BIGINT NOT NULL,
        [ContentType] NVARCHAR(100) NULL,
        [UploadedBy] NVARCHAR(255) NOT NULL,
        [UploadedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_TicketAttachments] PRIMARY KEY CLUSTERED ([TicketAttachmentId] ASC),
        CONSTRAINT [FK_TicketAttachments_Tickets] FOREIGN KEY([TicketId])
            REFERENCES [dbo].[Tickets] ([TicketId])
            ON DELETE CASCADE
    );

    CREATE INDEX IX_TicketAttachments_TicketId ON [dbo].[TicketAttachments]([TicketId]);
    CREATE INDEX IX_TicketAttachments_UploadedDate ON [dbo].[TicketAttachments]([UploadedDate] DESC);
END
GO

PRINT 'Database migration 05 completed successfully.'
GO
