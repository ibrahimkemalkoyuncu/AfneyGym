namespace AfneyGym.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public virtual Subscription? Subscription { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string Provider { get; set; } = "Manual";
    public string? ExternalPaymentId { get; set; }
    public string ExternalReference { get; set; } = Guid.NewGuid().ToString("N");
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public string? Note { get; set; }

    public virtual Invoice? Invoice { get; set; }
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}

