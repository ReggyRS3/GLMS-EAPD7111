IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029151235_InitialCreate'
)
BEGIN
    CREATE TABLE [Clients] (
        [ClientId] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [ContactEmail] nvarchar(max) NOT NULL,
        [ContactPhone] nvarchar(max) NULL,
        [Region] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([ClientId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029151235_InitialCreate'
)
BEGIN
    CREATE TABLE [Contracts] (
        [ContractId] int NOT NULL IDENTITY,
        [ClientId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ServiceLevel] nvarchar(50) NOT NULL,
        [SignedAgreementPath] nvarchar(max) NULL,
        CONSTRAINT [PK_Contracts] PRIMARY KEY ([ContractId]),
        CONSTRAINT [FK_Contracts_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029151235_InitialCreate'
)
BEGIN
    CREATE TABLE [ServiceRequests] (
        [ServiceRequestId] int NOT NULL IDENTITY,
        [ContractId] int NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [CostUsd] decimal(18,2) NOT NULL,
        [ExchangeRate] decimal(18,6) NOT NULL,
        [CostZar] decimal(18,2) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([ServiceRequestId]),
        CONSTRAINT [FK_ServiceRequests_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([ContractId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029151235_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ClientId', N'ContactEmail', N'ContactPhone', N'Name', N'Region') AND [object_id] = OBJECT_ID(N'[Clients]'))
        SET IDENTITY_INSERT [Clients] ON;
    EXEC(N'INSERT INTO [Clients] ([ClientId], [ContactEmail], [ContactPhone], [Name], [Region])
    VALUES (1, N''ops@kalaharifreight.co.za'', N''0115550101'', N''Kalahari Freight'', N''Africa''),
    (2, N''info@rhinecargo.de'', N''+4930555012'', N''Rhine Cargo GmbH'', N''Europe'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ClientId', N'ContactEmail', N'ContactPhone', N'Name', N'Region') AND [object_id] = OBJECT_ID(N'[Clients]'))
        SET IDENTITY_INSERT [Clients] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029151235_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ContractId', N'ClientId', N'EndDate', N'ServiceLevel', N'SignedAgreementPath', N'StartDate', N'Status') AND [object_id] = OBJECT_ID(N'[Contracts]'))
        SET IDENTITY_INSERT [Contracts] ON;
    EXEC(N'INSERT INTO [Contracts] ([ContractId], [ClientId], [EndDate], [ServiceLevel], [SignedAgreementPath], [StartDate], [Status])
    VALUES (1, 1, ''2026-12-31T00:00:00.0000000'', N''Premium'', NULL, ''2026-01-01T00:00:00.0000000'', N''Active''),
    (2, 1, ''2025-12-31T00:00:00.0000000'', N''Standard'', NULL, ''2025-01-01T00:00:00.0000000'', N''Expired''),
    (3, 2, ''2027-02-28T00:00:00.0000000'', N''Express'', NULL, ''2026-03-01T00:00:00.0000000'', N''OnHold'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ContractId', N'ClientId', N'EndDate', N'ServiceLevel', N'SignedAgreementPath', N'StartDate', N'Status') AND [object_id] = OBJECT_ID(N'[Contracts]'))
        SET IDENTITY_INSERT [Contracts] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029151235_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contracts_ClientId] ON [Contracts] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029151235_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceRequests_ContractId] ON [ServiceRequests] ([ContractId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029151235_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261029151235_InitialCreate', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030003935_AddAuditLogAndManagerAlerts'
)
BEGIN
    CREATE TABLE [ContractAuditLogs] (
        [ContractAuditLogId] int NOT NULL IDENTITY,
        [ContractId] int NOT NULL,
        [OldStatus] nvarchar(20) NOT NULL,
        [NewStatus] nvarchar(20) NOT NULL,
        [ChangedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ContractAuditLogs] PRIMARY KEY ([ContractAuditLogId]),
        CONSTRAINT [FK_ContractAuditLogs_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([ContractId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030003935_AddAuditLogAndManagerAlerts'
)
BEGIN
    CREATE TABLE [ManagerAlerts] (
        [ManagerAlertId] int NOT NULL IDENTITY,
        [ContractId] int NOT NULL,
        [Message] nvarchar(300) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDismissed] bit NOT NULL,
        CONSTRAINT [PK_ManagerAlerts] PRIMARY KEY ([ManagerAlertId]),
        CONSTRAINT [FK_ManagerAlerts_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([ContractId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030003935_AddAuditLogAndManagerAlerts'
)
BEGIN
    CREATE INDEX [IX_ContractAuditLogs_ContractId] ON [ContractAuditLogs] ([ContractId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030003935_AddAuditLogAndManagerAlerts'
)
BEGIN
    CREATE INDEX [IX_ManagerAlerts_ContractId] ON [ManagerAlerts] ([ContractId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030003935_AddAuditLogAndManagerAlerts'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261030003935_AddAuditLogAndManagerAlerts', N'8.0.33');
END;
GO

COMMIT;
GO

