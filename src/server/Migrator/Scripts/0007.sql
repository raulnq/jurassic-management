CREATE TABLE $schema$.[InvoiceToCollectionProcesses] (
    [CollectionId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_InvoiceToCollectionProcesses] PRIMARY KEY ([CollectionId]),
);

GO

CREATE TABLE $schema$.[InvoiceToCollectionProcessItems] (
    [CollectionId] UNIQUEIDENTIFIER NOT NULL,
    [InvoiceId] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_InvoiceToCollectionProcessItems] PRIMARY KEY ([CollectionId], [InvoiceId]),
    CONSTRAINT [FK_InvoiceToCollectionProcessItems_InvoiceToCollectionProcesses_CollectionId] FOREIGN KEY ([CollectionId]) REFERENCES $schema$.[InvoiceToCollectionProcesses] ([CollectionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_InvoiceToCollectionProcessItems_Invoices_Invoice] FOREIGN KEY ([InvoiceId]) REFERENCES $schema$.[Invoices] ([InvoiceId]) ON DELETE CASCADE,
);

GO

CREATE TABLE $schema$.[Collections] (
    [CollectionId] UNIQUEIDENTIFIER NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [Total] decimal(19,4) NOT NULL, 
    [ITF] decimal(19,4) NOT NULL, 
    [CreatedAt] datetimeoffset NOT NULL,
    [ConfirmedAt] date NULL,
    [Currency] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Collections] PRIMARY KEY ([CollectionId]),
);