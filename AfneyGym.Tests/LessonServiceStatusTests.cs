using AfneyGym.Common.Enums;
using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using AfneyGym.Service.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AfneyGym.Tests;

public class LessonServiceStatusTests
{
    [Fact]
    public async Task JoinLesson_Returns_NoActiveSubscription_When_UserHasNoActiveSubscription()
    {
        await using var context = BuildContext();
        var service = new LessonService(context, new TestNotificationService());
        var user = await SeedUserAsync(context);
        var lesson = await SeedLessonAsync(context);

        var result = await service.JoinLessonAsync(lesson.Id, user.Id);

        Assert.Equal(JoinLessonStatus.NoActiveSubscription, result);
    }

    [Fact]
    public async Task JoinLesson_Returns_LessonNotFound_When_LessonMissing()
    {
        await using var context = BuildContext();
        var service = new LessonService(context, new TestNotificationService());
        var user = await SeedUserAsync(context);
        await SeedActiveSubscriptionAsync(context, user.Id);

        var result = await service.JoinLessonAsync(Guid.NewGuid(), user.Id);

        Assert.Equal(JoinLessonStatus.LessonNotFound, result);
    }

    [Fact]
    public async Task JoinLesson_Returns_CapacityFull_When_LessonIsFull()
    {
        await using var context = BuildContext();
        var service = new LessonService(context, new TestNotificationService());

        var attendeeUser = await SeedUserAsync(context, "full@afney.test");
        var joiningUser = await SeedUserAsync(context, "joiner@afney.test");
        await SeedActiveSubscriptionAsync(context, joiningUser.Id);
        await SeedActiveSubscriptionAsync(context, attendeeUser.Id);

        var lesson = await SeedLessonAsync(context, capacity: 1);
        context.LessonAttendees.Add(new LessonAttendee { LessonId = lesson.Id, UserId = attendeeUser.Id });
        await context.SaveChangesAsync();

        var result = await service.JoinLessonAsync(lesson.Id, joiningUser.Id);

        Assert.Equal(JoinLessonStatus.CapacityFull, result);
    }

    [Fact]
    public async Task JoinLesson_Returns_AlreadyJoined_When_UserAlreadyRegistered()
    {
        await using var context = BuildContext();
        var service = new LessonService(context, new TestNotificationService());

        var user = await SeedUserAsync(context);
        await SeedActiveSubscriptionAsync(context, user.Id);

        var lesson = await SeedLessonAsync(context, capacity: 2);
        context.LessonAttendees.Add(new LessonAttendee { LessonId = lesson.Id, UserId = user.Id });
        await context.SaveChangesAsync();

        var result = await service.JoinLessonAsync(lesson.Id, user.Id);

        Assert.Equal(JoinLessonStatus.AlreadyJoined, result);
    }

    [Fact]
    public async Task JoinLesson_Returns_Success_AndCreatesAttendee_When_Valid()
    {
        await using var context = BuildContext();
        var service = new LessonService(context, new TestNotificationService());

        var user = await SeedUserAsync(context);
        await SeedActiveSubscriptionAsync(context, user.Id);
        var lesson = await SeedLessonAsync(context, capacity: 5);

        var result = await service.JoinLessonAsync(lesson.Id, user.Id);

        Assert.Equal(JoinLessonStatus.Success, result);
        Assert.True(await context.LessonAttendees.AnyAsync(x => x.LessonId == lesson.Id && x.UserId == user.Id));
    }

    [Fact]
    public async Task JoinLesson_Returns_TimeConflict_When_UserHasOverlappingLesson()
    {
        await using var context = BuildContext();
        var service = new LessonService(context, new TestNotificationService());

        var user = await SeedUserAsync(context);
        await SeedActiveSubscriptionAsync(context, user.Id);

        var existingLesson = await SeedLessonAsync(context, startOffsetHours: 3, durationHours: 1);
        var targetLesson = await SeedLessonAsync(context, startOffsetHours: 3.5, durationHours: 1);

        context.LessonAttendees.Add(new LessonAttendee { LessonId = existingLesson.Id, UserId = user.Id });
        await context.SaveChangesAsync();

        var result = await service.JoinLessonAsync(targetLesson.Id, user.Id);

        Assert.Equal(JoinLessonStatus.TimeConflict, result);
    }

    [Fact]
    public async Task JoinLesson_Returns_Success_When_LessonsTouchAtBoundary()
    {
        await using var context = BuildContext();
        var service = new LessonService(context, new TestNotificationService());

        var user = await SeedUserAsync(context);
        await SeedActiveSubscriptionAsync(context, user.Id);

        var existingLesson = await SeedLessonAsync(context, startOffsetHours: 3, durationHours: 1);
        var boundaryLesson = await SeedLessonAsync(context, startOffsetHours: 4, durationHours: 1);

        context.LessonAttendees.Add(new LessonAttendee { LessonId = existingLesson.Id, UserId = user.Id });
        await context.SaveChangesAsync();

        var result = await service.JoinLessonAsync(boundaryLesson.Id, user.Id);

        Assert.Equal(JoinLessonStatus.Success, result);
    }

    [Fact]
    public async Task CancelJoin_Returns_NotJoined_When_AttendanceMissing()
    {
        await using var context = BuildContext();
        var service = new LessonService(context, new TestNotificationService());

        var user = await SeedUserAsync(context);
        var lesson = await SeedLessonAsync(context);

        var result = await service.CancelJoinAsync(lesson.Id, user.Id);

        Assert.Equal(CancelJoinStatus.NotJoined, result);
    }

    [Fact]
    public async Task CancelJoin_Returns_Success_AndRemovesAttendance_When_Registered()
    {
        await using var context = BuildContext();
        var service = new LessonService(context, new TestNotificationService());

        var user = await SeedUserAsync(context);
        var lesson = await SeedLessonAsync(context);

        context.LessonAttendees.Add(new LessonAttendee { LessonId = lesson.Id, UserId = user.Id });
        await context.SaveChangesAsync();

        var result = await service.CancelJoinAsync(lesson.Id, user.Id);

        Assert.Equal(CancelJoinStatus.Success, result);
        Assert.False(await context.LessonAttendees.AnyAsync(x => x.LessonId == lesson.Id && x.UserId == user.Id));
    }

    private static AppDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"afney-test-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<User> SeedUserAsync(AppDbContext context, string email = "member@afney.test")
    {
        var gym = new Gym { Name = "Test Gym", City = "Istanbul" };
        var user = new User
        {
            FirstName = "Test",
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

    private static async Task SeedActiveSubscriptionAsync(AppDbContext context, Guid userId)
    {
        context.Subscriptions.Add(new Subscription
        {
            UserId = userId,
            PlanName = "1 Aylik Paket",
            StartDate = DateTime.Now.AddDays(-2),
            EndDate = DateTime.Now.AddDays(28),
            Status = SubscriptionStatus.Active,
            Price = 1000m
        });

        await context.SaveChangesAsync();
    }

    private static async Task<Lesson> SeedLessonAsync(AppDbContext context, int capacity = 3, double startOffsetHours = 3, double durationHours = 1)
    {
        var gym = new Gym { Name = "Lesson Gym", City = "Istanbul" };
        var trainer = new Trainer { FullName = "Trainer One", Email = $"trainer-{Guid.NewGuid():N}@afney.test" };

        context.Gyms.Add(gym);
        context.Trainers.Add(trainer);
        await context.SaveChangesAsync();

        var lesson = new Lesson
        {
            Name = "HIIT",
            Description = "Test lesson",
            StartTime = DateTime.Now.AddHours(startOffsetHours),
            EndTime = DateTime.Now.AddHours(startOffsetHours + durationHours),
            Capacity = capacity,
            GymId = gym.Id,
            TrainerId = trainer.Id
        };

        context.Lessons.Add(lesson);
        await context.SaveChangesAsync();
        return lesson;
    }

    private sealed class TestNotificationService : INotificationService
    {
        public Task SendToUserAsync(Guid userId, string title, string body, string category = "general") => Task.CompletedTask;
    }
}

