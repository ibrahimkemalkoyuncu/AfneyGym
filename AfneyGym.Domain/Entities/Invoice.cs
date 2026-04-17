namespace AfneyGym.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid PaymentId { get; set; }
    public virtual Payment? Payment { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public string HtmlReceipt { get; set; } = string.Empty;
    public string PdfRelativePath { get; set; } = string.Empty;
    public bool EmailSent { get; set; }
}

