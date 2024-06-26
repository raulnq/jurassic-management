﻿using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;

namespace WebAPI.CollaboratorPayments;

public enum CollaboratorPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Confirmed = 2,
    Canceled = 3
}

public class CollaboratorPayment
{
    public Guid CollaboratorPaymentId { get; private set; }
    public Guid CollaboratorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal NetSalary { get; private set; }
    public decimal Withholding { get; private set; }
    public CollaboratorPaymentStatus Status { get; private set; }
    public string? DocumentUrl { get; private set; }
    public string? Number { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public decimal ITF { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }

    private CollaboratorPayment() { }

    public CollaboratorPayment(Guid collaboratorPaymentId, Guid collaboratoId, decimal grossSalary, decimal withholdingPercentage, Currency currency, DateTimeOffset createdAt)
    {
        CollaboratorPaymentId = collaboratorPaymentId;
        CollaboratorId = collaboratoId;
        GrossSalary = grossSalary;
        Withholding = Math.Round((grossSalary * withholdingPercentage) / 100, 2, MidpointRounding.AwayFromZero);
        CreatedAt = createdAt;
        NetSalary = Math.Round(GrossSalary - Withholding, 2, MidpointRounding.AwayFromZero);
        Status = CollaboratorPaymentStatus.Pending;
        Currency = currency;
        Refresh();
    }

    public void Edit(decimal grossSalary, Currency currency, decimal withholdingPercentage)
    {
        GrossSalary = grossSalary;
        Withholding = (grossSalary * withholdingPercentage) / 100;
        NetSalary = GrossSalary - Withholding;
        Currency = currency;
        Refresh();
    }
    private void Refresh()
    {
        if (NetSalary >= 1000)
        {
            ITF = 0.00005m * NetSalary * 20m;
            ITF = Math.Round(ITF) / 20m;
        }
    }
    public void Pay(DateTime paidAt)
    {
        EnsureStatus(CollaboratorPaymentStatus.Pending);
        Status = CollaboratorPaymentStatus.Paid;
        PaidAt = paidAt;
    }

    public void Upload(string documentUrl)
    {
        DocumentUrl = documentUrl;
    }

    public void Confirm(DateTime confirmedAt, string number)
    {
        EnsureStatus(CollaboratorPaymentStatus.Paid);
        Number = number;
        Status = CollaboratorPaymentStatus.Confirmed;
        ConfirmedAt = confirmedAt;
    }

    public void Cancel(DateTimeOffset canceledAt)
    {
        EnsureStatus(CollaboratorPaymentStatus.Pending);
        Status = CollaboratorPaymentStatus.Canceled;
        CanceledAt = canceledAt;
    }

    public void EnsureStatus(CollaboratorPaymentStatus status)
    {
        if (status != Status)
        {
            throw new DomainException($"collaborator-payment-status-not-{status.ToString().ToLower()}");
        }
    }
}
