using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Common.Enums;
using AfneyGym.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AfneyGym.Tests;

public class SubscriptionRenewalServiceTests
{
    [Fact]
    public async Task ProcessDueSubscriptions_Should_Expire_WhenAutoRenewDisabled()
    {
        await using var context = BuildContext();
        var user = await SeedUserAsync(context, "expire@afney.test");
        var sub = await SeedSubscriptionAsync(context, user.Id, autoRenew: false, endDate: DateTime.Now.AddDays(-1));
        var service = new SubscriptionRenewalService(context, NullLogger<SubscriptionRenewalService>.Instance);

        var processed = await service.ProcessDueSubscriptionsAsync();

        Assert.Equal(1, processed);
        Assert.Equal(SubscriptionStatus.Expired, sub.Status);
        Assert.False(await context.Payments.AnyAsync(p => p.SubscriptionId == sub.Id));
    }

    [Fact]
    public async Task ProcessDueSubscriptions_Should_RenewAndCreateCompletedPayment_WhenAutoRenewEnabled()
    {
        await using var context = BuildContext();
        var user = await SeedUserAsync(context, "renew@afney.test");
        var sub = await SeedSubscriptionAsync(context, user.Id, autoRenew: true, endDate: DateTime.Now.AddHours(-2));
        var oldEndDate = sub.EndDate;
        var service = new SubscriptionRenewalService(context, NullLogger<SubscriptionRenewalService>.Instance);

        var processed = await service.ProcessDueSubscriptionsAsync();

        Assert.Equal(1, processed);
        Assert.True(sub.EndDate > oldEndDate);

        var payment = await context.Payments.FirstOrDefaultAsync(p => p.SubscriptionId == sub.Id);
        Assert.NotNull(payment);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("AutoRenew", payment.Provider);
    }

    [Fact]
    public async Task ProcessDueSubscriptions_Should_NotDuplicatePayment_ForSamePeriodReference()
    {
        await using var context = BuildContext();
        var user = await SeedUserAsync(context, "idempotent@afney.test");
        var sub = await SeedSubscriptionAsync(context, user.Id, autoRenew: true, endDate: DateTime.Now.AddMinutes(-30));
        var currentPeriodRef = $"AUTO-{sub.Id:N}-{DateTime.Now:yyyyMM}";

        context.Payments.Add(new Payment
        {
            SubscriptionId = sub.Id,
            UserId = sub.UserId,
            Amount = 1200m,
            Currency = "TRY",
            Provider = "AutoRenew",
            ExternalReference = currentPeriodRef,
            Status = PaymentStatus.Completed,
            PaidAt = DateTime.Now
        });
        await context.SaveChangesAsync();

        var service = new SubscriptionRenewalService(context, NullLogger<SubscriptionRenewalService>.Instance);
        var processed = await service.ProcessDueSubscriptionsAsync();

        Assert.Equal(1, processed); // EndDate normalize edilir ama yeni payment olusmaz
        var count = await context.Payments.CountAsync(p => p.SubscriptionId == sub.Id);
        Assert.Equal(1, count);
        Assert.True(sub.EndDate > DateTime.Now);
    }

    private static AppDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"afney-renew-test-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<User> SeedUserAsync(AppDbContext context, string email)
    {
        var gym = new Gym { Name = "Renew Gym", City = "Istanbul" };
        var user = new User
        {
            FirstName = "Renew",
            LastName = "Member",
            Email = email,
            PasswordHash = "hash",
            Role = UserRole.Member,
            Gym = gym
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static async Task<Subscription> SeedSubscriptionAsync(AppDbContext context, Guid userId, bool autoRenew, DateTime endDate)
    {
        var sub = new Subscription
        {
            UserId = userId,
            PlanName = "1 Aylik Paket",
            StartDate = endDate.AddMonths(-1),
            EndDate = endDate,
            Price = 1200m,
            AutoRenew = autoRenew,
            Status = SubscriptionStatus.Active
        };

        context.Subscriptions.Add(sub);
        await context.SaveChangesAsync();
        return sub;
    }
}


