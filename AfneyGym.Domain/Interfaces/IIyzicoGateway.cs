using AfneyGym.Common.DTOs;
using AfneyGym.Domain.Entities;

namespace AfneyGym.Domain.Interfaces;

public interface IIyzicoGateway
{
	Task<IyzicoCheckoutInitResultDto> CreateCheckoutFormUrlAsync(Subscription subscription, User user, CancellationToken cancellationToken = default);
	Task<IyzicoCheckoutVerificationResultDto> VerifyCallbackAsync(string token, string? conversationId, CancellationToken cancellationToken = default);
}

