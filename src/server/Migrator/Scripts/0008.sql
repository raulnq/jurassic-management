CREATE TABLE $schema$.[ProformaToCollaboratorPaymentProcesses] (
    [CollaboratorPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ProformaToCollaboratorPaymentProcesses] PRIMARY KEY ([CollaboratorPaymentId]),
);

GO

CREATE TABLE $schema$.[ProformaToCollaboratorPaymentProcessItems] (
    [CollaboratorPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [ProformaId] UNIQUEIDENTIFIER NOT NULL, 
    [Week] int NOT NULL, 
    [CollaboratorId] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_ProformaToCollaboratorPaymentProcessItems] PRIMARY KEY ([CollaboratorPaymentId], [ProformaId], [Week], [CollaboratorId]),
    CONSTRAINT [FK_ProformaToCollaboratorPaymentProcessItems_ProformaToCollaboratorPaymentProcesses_CollaboratorPaymentId] FOREIGN KEY ([CollaboratorPaymentId]) REFERENCES $schema$.[ProformaToCollaboratorPaymentProcesses] ([CollaboratorPaymentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProformaToCollaboratorPaymentProcessItems_ProformaWeekWorkItem] FOREIGN KEY ([ProformaId],[Week],[CollaboratorId]) REFERENCES $schema$.[ProformaWeekWorkItems] ([ProformaId],[Week],[CollaboratorId]) ON DELETE CASCADE,
);

GO

CREATE TABLE $schema$.[CollaboratorPayments] (
    [CollaboratorPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [ProformaId] UNIQUEIDENTIFIER NOT NULL, 
    [Week] int NOT NULL, 
    [CollaboratorId] UNIQUEIDENTIFIER NOT NULL, 
    [Status] nvarchar(50) NOT NULL,
    [GrossSalary] decimal(19,4) NOT NULL, 
    [NetSalary] decimal(19,4) NOT NULL, 
    [ITF] decimal(19,4) NOT NULL, 
    [Withholding] decimal(19,4) NOT NULL, 
    [CreatedAt] datetimeoffset NOT NULL,
    [PaidAt] date NULL,
    [DocumentUrl] nvarchar(500) NULL,
    [ConfirmedAt] date NULL,
    [Currency] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_CollaboratorPayments] PRIMARY KEY ([CollaboratorPaymentId]),
);