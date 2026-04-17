using AfneyGym.Data.Context;
using AfneyGym.Service.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AfneyGym.Tests;

public class DashboardHeroVariantTests
{
    [Fact]
    public async Task TrackHeroVariantExposure_Should_CreateUniqueVisitorRows_AndPopulateAnalyticsCounts()
    {
        await using var context = BuildContext();
        var service = new DashboardService(context);

        await service.TrackHeroVariantExposureAsync("visitor-a", "a");
        await service.TrackHeroVariantExposureAsync("visitor-b", "b");
        await service.TrackHeroVariantExposureAsync("visitor-a", "a"); // duplicate call

        var analytics = await service.GetAnalyticsAsync();

        Assert.Equal(1, analytics.HeroVariantACount);
        Assert.Equal(1, analytics.HeroVariantBCount);
        Assert.Equal(2, await context.HeroVariantExposures.CountAsync());
    }

    [Fact]
    public async Task TrackHeroVariantExposure_Should_UpdateVariant_WhenVisitorChoiceChanges()
    {
        await using var context = BuildContext();
        var service = new DashboardService(context);

        await service.TrackHeroVariantExposureAsync("visitor-x", "a");
        await service.TrackHeroVariantExposureAsync("visitor-x", "b");

        var analytics = await service.GetAnalyticsAsync();

        Assert.Equal(0, analytics.HeroVariantACount);
        Assert.Equal(1, analytics.HeroVariantBCount);

        var stored = await context.HeroVariantExposures.SingleAsync(x => x.VisitorId == "visitor-x");
        Assert.Equal("b", stored.Variant);
    }

    [Fact]
    public async Task TrackHeroVariantExposure_Should_DefaultToA_WhenVariantInvalid()
    {
        await using var context = BuildContext();
        var service = new DashboardService(context);

        await service.TrackHeroVariantExposureAsync("visitor-invalid", "x");

        var stored = await context.HeroVariantExposures.SingleAsync(x => x.VisitorId == "visitor-invalid");
        Assert.Equal("a", stored.Variant);
    }

    private static AppDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"afney-hero-test-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }
}

