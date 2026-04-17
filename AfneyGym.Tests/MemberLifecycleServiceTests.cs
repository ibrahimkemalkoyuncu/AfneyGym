using AfneyGym.Common.DTOs;
using AfneyGym.Common.Enums;
using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using AfneyGym.Service.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AfneyGym.Tests;

public class MemberLifecycleServiceTests
{
    [Fact]
    public async Task CheckInAndCheckOut_Should_CreateAndCloseSession()
    {
        await using var context = BuildContext();
        var email = new FakeEmailService();
        var service = new MemberLifecycleService(context, email);

        var user = await SeedMemberAsync(context, "checkin@afney.test", createdAt: DateTime.Now.AddDays(-10));
        await SeedActiveSubscriptionAsync(context, user.Id, DateTime.Now.AddDays(15));

        var checkInOk = await service.CheckInAsync(user.Id);
        var checkOutOk = await service.CheckOutAsync(user.Id);

        Assert.True(checkInOk);
        Assert.True(checkOutOk);
        Assert.True(await context.GymCheckIns.AnyAsync(c => c.UserId == user.Id && c.CheckOutTime != null));
    }

    [Fact]
    public async Task UpdateLifecycleStatus_Should_Set_AtRisk_ForInactiveMember()
    {
        await using var context = BuildContext();
        var service = new MemberLifecycleService(context, new FakeEmailService());

        var user = await SeedMemberAsync(context, "atrisk@afney.test", createdAt: DateTime.Now.AddDays(-40));
        await SeedActiveSubscriptionAsync(context, user.Id, DateTime.Now.AddDays(20));

        var stage = await service.UpdateLifecycleStatusAsync(user.Id);

        Assert.Equal(MemberLifecycleStage.AtRisk, stage);
        var status = await context.UserLifecycleStatuses.FirstOrDefaultAsync(s => s.UserId == user.Id);
        Assert.NotNull(status);
        Assert.Equal(MemberLifecycleStage.AtRisk, status!.CurrentStage);
    }

    [Fact]
    public async Task GetChurnRiskMembers_Should_ReturnUsersWithSoonExpiringSubs()
    {
        await using var context = BuildContext();
        var service = new MemberLifecycleService(context, new FakeEmailService());

        var risky = await SeedMemberAsync(context, "risk-sub@afney.test", DateTime.Now.AddDays(-60));
        var safe = await SeedMemberAsync(context, "safe-sub@afney.test", DateTime.Now.AddDays(-60));

        await SeedActiveSubscriptionAsync(context, risky.Id, DateTime.Now.AddDays(3));
        await SeedActiveSubscriptionAsync(context, safe.Id, DateTime.Now.AddDays(20));

        var riskIds = await service.GetChurnRiskMembersAsync();

        Assert.Contains(risky.Id, riskIds);
        Assert.DoesNotContain(safe.Id, riskIds);
    }

    [Fact]
    public async Task AddBodyMetric_Should_PersistAndSummarize()
    {
        await using var context = BuildContext();
        var service = new MemberLifecycleService(context, new FakeEmailService());

        var user = await SeedMemberAsync(context, "metric@afney.test", DateTime.Now.AddDays(-100));

        await service.AddBodyMetricAsync(user.Id, new BodyMetricCreateDto
        {
            Weight = 85,
            BodyFatPercentage = 24,
            MeasurementDate = DateTime.Now.AddMonths(-1)
        });

        await service.AddBodyMetricAsync(user.Id, new BodyMetricCreateDto
        {
            Weight = 82,
            BodyFatPercentage = 21,
            MeasurementDate = DateTime.Now
        });

        var summary = await service.GetLatestBodyMetricSummaryAsync(user.Id, 1);

        Assert.NotNull(summary);
        Assert.Equal(82, summary!.Weight);
        Assert.Equal(-3, summary.WeightChangeMonth);
    }

    private static AppDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"afney-lifecycle-test-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<User> SeedMemberAsync(AppDbContext context, string email, DateTime createdAt)
    {
        var gym = new Gym { Name = "Lifecycle Gym", City = "Istanbul" };
        var user = new User
        {
            FirstName = "Test",
            LastName = "Member",
            Email = email,
            PasswordHash = "hash",
            Role = UserRole.Member,
            Gym = gym,
            CreatedAt = createdAt
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static async Task SeedActiveSubscriptionAsync(AppDbContext context, Guid userId, DateTime endDate)
    {
        context.Subscriptions.Add(new Subscription
        {
            UserId = userId,
            PlanName = "1 Aylik Paket",
            StartDate = DateTime.Now.AddDays(-5),
            EndDate = endDate,
            Status = SubscriptionStatus.Active,
            Price = 1200m
        });

        await context.SaveChangesAsync();
    }

    private sealed class FakeEmailService : IEmailService
    {
        public Task SendEmailAsync(string toEmail, string subject, string body) => Task.CompletedTask;
        public Task SendLessonReminderAsync(string toEmail, string memberName, string lessonName, DateTime lessonTime) => Task.CompletedTask;
    }
}

