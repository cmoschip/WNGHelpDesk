-- =============================================
-- IT Help Desk Ticket Tracking System
-- Database Creation Script
-- =============================================

USE master;
GO

-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ITHelpDesk')
BEGIN
    CREATE DATABASE ITHelpDesk;
    PRINT 'Database ITHelpDesk created successfully.';
END
ELSE
BEGIN
    PRINT 'Database ITHelpDesk already exists.';
END
GO

USE ITHelpDesk;
GO
