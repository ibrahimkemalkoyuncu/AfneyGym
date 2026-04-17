using System.Globalization;
using AfneyGym.Common.DTOs;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;

namespace AfneyGym.Service.Services;

public class IyzicoGatewayService : IIyzicoGateway
{
	private readonly IyzicoSettings _settings;

	public IyzicoGatewayService(IOptions<IyzicoSettings> settings)
	{
		_settings = settings.Value;
	}

	public async Task<IyzicoCheckoutInitResultDto> CreateCheckoutFormUrlAsync(Subscription subscription, User user, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(_settings.ApiKey) || string.IsNullOrWhiteSpace(_settings.SecretKey))
		{
			return new IyzicoCheckoutInitResultDto
			{
				IsSuccess = false,
				ErrorMessage = "iyzico ApiKey/SecretKey ayarlari eksik."
			};
		}

		var conversationId = $"SUB-{subscription.Id:N}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
		var callbackUrl = string.IsNullOrWhiteSpace(_settings.CallbackUrl)
			? "https://localhost:5001/subscription/iyzico-callback"
			: _settings.CallbackUrl;

		var request = new CreateCheckoutFormInitializeRequest
		{
			Locale = Locale.TR.ToString(),
			ConversationId = conversationId,
			Price = FormatPrice(subscription.Price),
			PaidPrice = FormatPrice(subscription.Price),
			Currency = Currency.TRY.ToString(),
			BasketId = subscription.Id.ToString("N"),
			PaymentGroup = PaymentGroup.SUBSCRIPTION.ToString(),
			CallbackUrl = callbackUrl,
			EnabledInstallments = new List<int> { 1 }
		};

		request.Buyer = new Buyer
		{
			Id = user.Id.ToString("N"),
			Name = Safe(user.FirstName, "Uye"),
			Surname = Safe(user.LastName, "Kullanici"),
			Email = Safe(user.Email, "member@example.com"),
			IdentityNumber = "11111111111",
			RegistrationAddress = "AfneyGym Uye Kaydi",
			Ip = "127.0.0.1",
			City = "Istanbul",
			Country = "Turkey",
			ZipCode = "34000"
		};

		var address = new Address
		{
			ContactName = $"{Safe(user.FirstName, "Uye")} {Safe(user.LastName, "Kullanici")}",
			City = "Istanbul",
			Country = "Turkey",
			Description = "AfneyGym Dijital Uyelik",
			ZipCode = "34000"
		};

		request.BillingAddress = address;
		request.ShippingAddress = address;
		request.BasketItems = new List<BasketItem>
		{
			new()
			{
				Id = subscription.Id.ToString("N"),
				Name = Safe(subscription.PlanName, "AfneyGym Uyelik"),
				Category1 = "Membership",
				ItemType = BasketItemType.VIRTUAL.ToString(),
				Price = FormatPrice(subscription.Price)
			}
		};

		try
		{
			var response = await Task.Run(() => CheckoutFormInitialize.Create(request, BuildOptions()), cancellationToken);
			var isSuccess = response != null
							&& string.Equals(response.Status, "success", StringComparison.OrdinalIgnoreCase)
							&& !string.IsNullOrWhiteSpace(response.PaymentPageUrl);

			return new IyzicoCheckoutInitResultDto
			{
				IsSuccess = isSuccess,
				CheckoutFormUrl = response?.PaymentPageUrl ?? string.Empty,
				ConversationId = response?.ConversationId ?? conversationId,
				Token = response?.Token ?? string.Empty,
				ErrorMessage = isSuccess ? string.Empty : GetErrorText(response?.ErrorCode, response?.ErrorMessage)
			};
		}
		catch (Exception ex)
		{
			return new IyzicoCheckoutInitResultDto
			{
				IsSuccess = false,
				ConversationId = conversationId,
				ErrorMessage = $"iyzico checkout baslatilamadi: {ex.Message}"
			};
		}
	}

	public async Task<IyzicoCheckoutVerificationResultDto> VerifyCallbackAsync(string token, string? conversationId, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(token))
		{
			return new IyzicoCheckoutVerificationResultDto
			{
				IsSuccess = false,
				ConversationId = conversationId ?? string.Empty,
				ErrorMessage = "iyzico callback token bos geldi."
			};
		}

		var request = new RetrieveCheckoutFormRequest
		{
			Locale = Locale.TR.ToString(),
			ConversationId = conversationId ?? string.Empty,
			Token = token
		};

		try
		{
			var response = await Task.Run(() => CheckoutForm.Retrieve(request, BuildOptions()), cancellationToken);

			var statusSuccess = string.Equals(response?.Status, "success", StringComparison.OrdinalIgnoreCase);
			var paymentSuccess = string.Equals(response?.PaymentStatus, "SUCCESS", StringComparison.OrdinalIgnoreCase);

			decimal.TryParse(response?.PaidPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var paidPrice);

			return new IyzicoCheckoutVerificationResultDto
			{
				IsSuccess = statusSuccess && paymentSuccess,
				ConversationId = response?.ConversationId ?? conversationId ?? string.Empty,
				PaymentId = response?.PaymentId ?? string.Empty,
				PaymentStatus = response?.PaymentStatus ?? string.Empty,
				PaidPrice = paidPrice,
				Currency = response?.Currency ?? string.Empty,
				ErrorMessage = statusSuccess && paymentSuccess
					? string.Empty
					: GetErrorText(response?.ErrorCode, response?.ErrorMessage)
			};
		}
		catch (Exception ex)
		{
			return new IyzicoCheckoutVerificationResultDto
			{
				IsSuccess = false,
				ConversationId = conversationId ?? string.Empty,
				ErrorMessage = $"iyzico callback dogrulama hatasi: {ex.Message}"
			};
		}
	}

	private Iyzipay.Options BuildOptions() => new()
	{
		ApiKey = _settings.ApiKey,
		SecretKey = _settings.SecretKey,
		BaseUrl = _settings.BaseUrl
	};

	private static string FormatPrice(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);

	private static string Safe(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;

	private static string GetErrorText(string? errorCode, string? errorMessage)
	{
		if (string.IsNullOrWhiteSpace(errorCode) && string.IsNullOrWhiteSpace(errorMessage))
			return "iyzico islemi basarisiz dondu.";

		return $"{errorCode} {errorMessage}".Trim();
	}
}

