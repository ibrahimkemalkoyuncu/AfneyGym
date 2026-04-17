namespace AfneyGym.Common.DTOs;

public class IyzicoCheckoutInitResultDto
{
    public bool IsSuccess { get; set; }
    public string CheckoutFormUrl { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class IyzicoCheckoutVerificationResultDto
{
    public bool IsSuccess { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal PaidPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

