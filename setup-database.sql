-- ============================================================
-- TaskBoard Database Setup Script
-- Creates the TaskBoard database and all tables (idempotent)
-- Run against (localdb)\MSSQLLocalDB or any SQL Server instance
-- ============================================================
SET QUOTED_IDENTIFIER ON;
GO

USE [master]
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TaskBoard')
BEGIN
    CREATE DATABASE [TaskBoard];
END
GO

USE [TaskBoard]
GO

-- ============================================================
-- TABLES
-- ============================================================

-- Users
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Users')
BEGIN
    CREATE TABLE dbo.Users (
        Id              UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        Email           NVARCHAR(256)       NOT NULL,
        PasswordHash    NVARCHAR(512)       NOT NULL,
        FirstName       NVARCHAR(100)       NOT NULL,
        LastName        NVARCHAR(100)       NOT NULL,
        IsActive        BIT                 NOT NULL DEFAULT 1,
        DateCreated     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        DateUpdated     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Users PRIMARY KEY (Id),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
END
GO

-- Roles
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Roles')
BEGIN
    CREATE TABLE dbo.Roles (
        Id      UNIQUEIDENTIFIER    NOT NULL,
        Name    NVARCHAR(50)        NOT NULL,
        CONSTRAINT PK_Roles PRIMARY KEY (Id),
        CONSTRAINT UQ_Roles_Name UNIQUE (Name)
    );
END
GO

-- UserRoles
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'UserRoles')
BEGIN
    CREATE TABLE dbo.UserRoles (
        UserId  UNIQUEIDENTIFIER    NOT NULL,
        RoleId  UNIQUEIDENTIFIER    NOT NULL,
        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id)
    );
END
GO

-- Projects
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Projects')
BEGIN
    CREATE TABLE dbo.Projects (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        Name        NVARCHAR(200)       NOT NULL,
        Description NVARCHAR(2000)      NULL,
        OwnerId     UNIQUEIDENTIFIER    NOT NULL,
        IsArchived  BIT                 NOT NULL DEFAULT 0,
        DateCreated DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        DateUpdated DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Projects PRIMARY KEY (Id),
        CONSTRAINT FK_Projects_OwnerId FOREIGN KEY (OwnerId) REFERENCES dbo.Users (Id)
    );
END
GO

-- ProjectMembers
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ProjectMembers')
BEGIN
    CREATE TABLE dbo.ProjectMembers (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        ProjectId   UNIQUEIDENTIFIER    NOT NULL,
        UserId      UNIQUEIDENTIFIER    NOT NULL,
        Role        NVARCHAR(50)        NOT NULL,
        DateAdded   DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ProjectMembers PRIMARY KEY (Id),
        CONSTRAINT UQ_ProjectMembers_ProjectUser UNIQUE (ProjectId, UserId),
        CONSTRAINT FK_ProjectMembers_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (Id),
        CONSTRAINT FK_ProjectMembers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );
END
GO

-- TaskItems
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TaskItems')
BEGIN
    CREATE TABLE dbo.TaskItems (
        Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        ProjectId           UNIQUEIDENTIFIER    NOT NULL,
        Title               NVARCHAR(300)       NOT NULL,
        Description         NVARCHAR(4000)      NULL,
        Status              NVARCHAR(50)        NOT NULL DEFAULT 'Todo',
        Priority            NVARCHAR(50)        NULL,
        DueDate             DATETIME2           NULL,
        CreatedByUserId     UNIQUEIDENTIFIER    NOT NULL,
        RowVersion          INT                 NOT NULL DEFAULT 1,
        DateCreated         DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        DateUpdated         DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_TaskItems PRIMARY KEY (Id),
        CONSTRAINT FK_TaskItems_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (Id),
        CONSTRAINT FK_TaskItems_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users (Id)
    );

    CREATE INDEX IX_TaskItems_ProjectId ON dbo.TaskItems (ProjectId);
    CREATE INDEX IX_TaskItems_Status ON dbo.TaskItems (Status);
END
GO

-- TaskAssignments
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TaskAssignments')
BEGIN
    CREATE TABLE dbo.TaskAssignments (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        TaskItemId  UNIQUEIDENTIFIER    NOT NULL,
        UserId      UNIQUEIDENTIFIER    NOT NULL,
        DateAssigned DATETIME2          NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_TaskAssignments PRIMARY KEY (Id),
        CONSTRAINT UQ_TaskAssignments_TaskUser UNIQUE (TaskItemId, UserId),
        CONSTRAINT FK_TaskAssignments_TaskItems FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems (Id),
        CONSTRAINT FK_TaskAssignments_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );
END
GO

-- TaskComments
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TaskComments')
BEGIN
    CREATE TABLE dbo.TaskComments (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        TaskItemId  UNIQUEIDENTIFIER    NOT NULL,
        UserId      UNIQUEIDENTIFIER    NOT NULL,
        Content     NVARCHAR(4000)      NOT NULL,
        DateCreated DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_TaskComments PRIMARY KEY (Id),
        CONSTRAINT FK_TaskComments_TaskItems FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems (Id),
        CONSTRAINT FK_TaskComments_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );

    CREATE INDEX IX_TaskComments_TaskItemId ON dbo.TaskComments (TaskItemId);
END
GO

-- TaskHistory
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TaskHistory')
BEGIN
    CREATE TABLE dbo.TaskHistory (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        TaskItemId  UNIQUEIDENTIFIER    NOT NULL,
        UserId      UNIQUEIDENTIFIER    NOT NULL,
        Action      NVARCHAR(100)       NOT NULL,
        OldValue    NVARCHAR(1000)      NULL,
        NewValue    NVARCHAR(1000)      NULL,
        DateCreated DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_TaskHistory PRIMARY KEY (Id),
        CONSTRAINT FK_TaskHistory_TaskItems FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems (Id),
        CONSTRAINT FK_TaskHistory_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );

    CREATE INDEX IX_TaskHistory_TaskItemId ON dbo.TaskHistory (TaskItemId);
END
GO

-- UserNotifications
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'UserNotifications')
BEGIN
    CREATE TABLE dbo.UserNotifications (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        UserId      UNIQUEIDENTIFIER    NOT NULL,
        Type        NVARCHAR(50)        NOT NULL,
        Title       NVARCHAR(300)       NOT NULL,
        Message     NVARCHAR(2000)      NOT NULL,
        ReferenceId UNIQUEIDENTIFIER    NULL,
        IsRead      BIT                 NOT NULL DEFAULT 0,
        DateCreated DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_UserNotifications PRIMARY KEY (Id),
        CONSTRAINT FK_UserNotifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );

    CREATE INDEX IX_UserNotifications_UserId ON dbo.UserNotifications (UserId);
END
GO

-- DomainEventOutbox
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'DomainEventOutbox')
BEGIN
    CREATE TABLE dbo.DomainEventOutbox (
        Id              UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
        EventName       NVARCHAR(200)       NOT NULL,
        EntityId        UNIQUEIDENTIFIER    NOT NULL,
        EntityType      NVARCHAR(100)       NOT NULL,
        Payload         NVARCHAR(MAX)       NOT NULL,
        Status          NVARCHAR(50)        NOT NULL DEFAULT 'Pending',
        Attempts        INT                 NOT NULL DEFAULT 0,
        DateCreated     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        DateProcessed   DATETIME2           NULL,
        CONSTRAINT PK_DomainEventOutbox PRIMARY KEY (Id)
    );

    CREATE INDEX IX_DomainEventOutbox_Status ON dbo.DomainEventOutbox (Status)
    WHERE Status = 'Pending';
END
GO

-- NotificationIndex
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'NotificationIndex')
BEGIN
    CREATE TABLE dbo.NotificationIndex (
        EntityId            UNIQUEIDENTIFIER    NOT NULL,
        EntityType          NVARCHAR(100)       NOT NULL,
        CriticalFieldsHash  NVARCHAR(64)        NOT NULL,
        DateUpdated         DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_NotificationIndex PRIMARY KEY (EntityId, EntityType)
    );
END
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Roles
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Id = 'A1000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO dbo.Roles (Id, Name) VALUES
        ('A1000000-0000-0000-0000-000000000001', 'Admin'),
        ('A1000000-0000-0000-0000-000000000002', 'Manager'),
        ('A1000000-0000-0000-0000-000000000003', 'Member');
END
GO

-- Users (all with same bcrypt hash for Password123!)
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = 'B1000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO dbo.Users (Id, Email, PasswordHash, FirstName, LastName, IsActive)
    VALUES
        ('B1000000-0000-0000-0000-000000000001', 'admin@taskboard.local',   '$2a$11$coXE/FGO9I0Rm4u.yQt/KuxHyS8dVrggKCdGk0i03EeuFLWv4Z2X6', 'Alice',   'Admin',     1),
        ('B1000000-0000-0000-0000-000000000002', 'manager@taskboard.local', '$2a$11$coXE/FGO9I0Rm4u.yQt/KuxHyS8dVrggKCdGk0i03EeuFLWv4Z2X6', 'Bob',     'Manager',   1),
        ('B1000000-0000-0000-0000-000000000003', 'member1@taskboard.local', '$2a$11$coXE/FGO9I0Rm4u.yQt/KuxHyS8dVrggKCdGk0i03EeuFLWv4Z2X6', 'Charlie', 'Member',    1),
        ('B1000000-0000-0000-0000-000000000004', 'member2@taskboard.local', '$2a$11$coXE/FGO9I0Rm4u.yQt/KuxHyS8dVrggKCdGk0i03EeuFLWv4Z2X6', 'Diana',   'Developer', 1);
END
GO

-- UserRoles
IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = 'B1000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId)
    VALUES
        ('B1000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000001'), -- Alice = Admin
        ('B1000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000002'), -- Bob = Manager
        ('B1000000-0000-0000-0000-000000000003', 'A1000000-0000-0000-0000-000000000003'), -- Charlie = Member
        ('B1000000-0000-0000-0000-000000000004', 'A1000000-0000-0000-0000-000000000003'); -- Diana = Member
END
GO

-- Projects
IF NOT EXISTS (SELECT 1 FROM dbo.Projects WHERE Id = 'C1000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO dbo.Projects (Id, Name, Description, OwnerId, IsArchived)
    VALUES
        ('C1000000-0000-0000-0000-000000000001', 'Website Redesign',  'Complete overhaul of the company website with modern design and improved UX.', 'B1000000-0000-0000-0000-000000000002', 0),
        ('C1000000-0000-0000-0000-000000000002', 'Mobile App v2',     'Second version of the mobile application with new features and performance improvements.', 'B1000000-0000-0000-0000-000000000002', 0);
END
GO

-- ProjectMembers
IF NOT EXISTS (SELECT 1 FROM dbo.ProjectMembers WHERE ProjectId = 'C1000000-0000-0000-0000-000000000001' AND UserId = 'B1000000-0000-0000-0000-000000000002')
BEGIN
    -- Website Redesign: Bob=Manager, Charlie=Contributor, Diana=Contributor
    INSERT INTO dbo.ProjectMembers (ProjectId, UserId, Role)
    VALUES
        ('C1000000-0000-0000-0000-000000000001', 'B1000000-0000-0000-0000-000000000002', 'Manager'),
        ('C1000000-0000-0000-0000-000000000001', 'B1000000-0000-0000-0000-000000000003', 'Contributor'),
        ('C1000000-0000-0000-0000-000000000001', 'B1000000-0000-0000-0000-000000000004', 'Contributor'),

        -- Mobile App v2: Bob=Manager, Charlie=Contributor
        ('C1000000-0000-0000-0000-000000000002', 'B1000000-0000-0000-0000-000000000002', 'Manager'),
        ('C1000000-0000-0000-0000-000000000002', 'B1000000-0000-0000-0000-000000000003', 'Contributor');
END
GO

-- TaskItems (5 tasks)
IF NOT EXISTS (SELECT 1 FROM dbo.TaskItems WHERE Id = 'D1000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO dbo.TaskItems (Id, ProjectId, Title, Description, Status, Priority, CreatedByUserId)
    VALUES
        ('D1000000-0000-0000-0000-000000000001', 'C1000000-0000-0000-0000-000000000001', 'Design new homepage layout',    'Create wireframes and mockups for the new homepage design.',           'InProgress', 'High',   'B1000000-0000-0000-0000-000000000002'),
        ('D1000000-0000-0000-0000-000000000002', 'C1000000-0000-0000-0000-000000000001', 'Implement responsive CSS',      'Update all stylesheets to support mobile and tablet viewports.',        'Todo',       'Medium', 'B1000000-0000-0000-0000-000000000002'),
        ('D1000000-0000-0000-0000-000000000003', 'C1000000-0000-0000-0000-000000000001', 'Set up CI/CD pipeline',         'Configure GitHub Actions for automated build, test, and deployment.',   'Done',       'High',   'B1000000-0000-0000-0000-000000000002'),
        ('D1000000-0000-0000-0000-000000000004', 'C1000000-0000-0000-0000-000000000002', 'Create mobile navigation bar',  'Implement bottom navigation bar for iOS and Android.',                  'Todo',       'High',   'B1000000-0000-0000-0000-000000000002'),
        ('D1000000-0000-0000-0000-000000000005', 'C1000000-0000-0000-0000-000000000002', 'Integrate push notifications',  'Set up FCM and APNS for push notification delivery.',                  'InProgress', 'Medium', 'B1000000-0000-0000-0000-000000000002');
END
GO

-- TaskAssignments: D1=Charlie, D2=Diana, D5=Charlie
IF NOT EXISTS (SELECT 1 FROM dbo.TaskAssignments WHERE TaskItemId = 'D1000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO dbo.TaskAssignments (TaskItemId, UserId)
    VALUES
        ('D1000000-0000-0000-0000-000000000001', 'B1000000-0000-0000-0000-000000000003'), -- D1 -> Charlie
        ('D1000000-0000-0000-0000-000000000002', 'B1000000-0000-0000-0000-000000000004'), -- D2 -> Diana
        ('D1000000-0000-0000-0000-000000000005', 'B1000000-0000-0000-0000-000000000003'); -- D5 -> Charlie
END
GO

-- Seed task history entries
IF NOT EXISTS (SELECT 1 FROM dbo.TaskHistory WHERE TaskItemId = 'D1000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO dbo.TaskHistory (TaskItemId, UserId, Action, OldValue, NewValue)
    VALUES
        ('D1000000-0000-0000-0000-000000000001', 'B1000000-0000-0000-0000-000000000002', 'Created', NULL, 'Design new homepage layout'),
        ('D1000000-0000-0000-0000-000000000001', 'B1000000-0000-0000-0000-000000000002', 'StatusChanged', 'Todo', 'InProgress'),
        ('D1000000-0000-0000-0000-000000000002', 'B1000000-0000-0000-0000-000000000002', 'Created', NULL, 'Implement responsive CSS'),
        ('D1000000-0000-0000-0000-000000000003', 'B1000000-0000-0000-0000-000000000002', 'Created', NULL, 'Set up CI/CD pipeline'),
        ('D1000000-0000-0000-0000-000000000003', 'B1000000-0000-0000-0000-000000000002', 'StatusChanged', 'Todo', 'Done'),
        ('D1000000-0000-0000-0000-000000000004', 'B1000000-0000-0000-0000-000000000002', 'Created', NULL, 'Create mobile navigation bar'),
        ('D1000000-0000-0000-0000-000000000005', 'B1000000-0000-0000-0000-000000000002', 'Created', NULL, 'Integrate push notifications'),
        ('D1000000-0000-0000-0000-000000000005', 'B1000000-0000-0000-0000-000000000002', 'StatusChanged', 'Todo', 'InProgress');
END
GO

PRINT 'TaskBoard database setup complete.'
GO
