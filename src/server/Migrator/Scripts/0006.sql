CREATE TABLE $schema$.[ProformaToInvoiceProcesses] (
    [InvoiceId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ProformaToInvoiceProcesses] PRIMARY KEY ([InvoiceId]),
);

GO

CREATE TABLE $schema$.[ProformaToInvoiceProcessItems] (
    [ProformaId] UNIQUEIDENTIFIER NOT NULL,
    [InvoiceId] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_ProformaToInvoiceProcessItems] PRIMARY KEY ([ProformaId], [InvoiceId]),
    CONSTRAINT [FK_ProformaToInvoiceProcessItems_ProformaToInvoiceProcesses_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES $schema$.[ProformaToInvoiceProcesses] ([InvoiceId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProformaToInvoiceProcessItems_Proformas_ProformaId] FOREIGN KEY ([ProformaId]) REFERENCES $schema$.[Proformas] ([ProformaId]) ON DELETE CASCADE,
);

GO

CREATE TABLE $schema$.[Invoices] (
    [InvoiceId] UNIQUEIDENTIFIER NOT NULL,
    [DocumentUrl] nvarchar(500) NULL,
    [Status] nvarchar(50) NOT NULL,
    [Total] decimal(19,4) NOT NULL, 
    [Taxes] decimal(19,4) NOT NULL,
    [SubTotal] decimal(19,4) NOT NULL, 
    [CreatedAt] datetimeoffset NOT NULL,
    [IssueAt] date NULL,
    [Currency] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([InvoiceId]),
);