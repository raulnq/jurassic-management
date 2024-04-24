namespace WebAPI.Infrastructure.EntityFramework;

public static class Tables
{
    public static Table CollaboratorRoles = new Table("CollaboratorRoles");

    public static Table Collaborators = new Table("Collaborators");

    public static Table Periods = new Table("Periods");

    public static Table Projects = new Table("Projects");

    public static Table Clients = new Table("Clients");

    public static Table Proformas = new Table("Proformas");

    public static Table ProformaWeeks = new Table("ProformaWeeks");

    public static Table ProformaWeekWorkItems = new Table("ProformaWeekWorkItems");

    public static Table ProformaToInvoiceProcesses = new Table("ProformaToInvoiceProcesses");

    public static Table ProformaToInvoiceProcessItems = new Table("ProformaToInvoiceProcessItems");

    public static Table Invoices = new Table("Invoices");

    public static Table InvoiceToCollectionProcesses = new Table("InvoiceToCollectionProcesses");

    public static Table InvoiceToCollectionProcessItems = new Table("InvoiceToCollectionProcessItems");

    public static Table Collections = new Table("Collections");

    public static Table CollaboratorPayments = new Table("CollaboratorPayments");

    public static Table ProformaToCollaboratorPaymentProcesses = new Table("ProformaToCollaboratorPaymentProcesses");

    public static Table ProformaToCollaboratorPaymentProcessItems = new Table("ProformaToCollaboratorPaymentProcessItems");

}
