using Microsoft.Playwright;
using NUnit.Framework;

namespace AfneyGym.E2E;

[TestFixture]
public class SmokeFlowTests
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        if (!IsE2EEnabled())
        {
            Assert.Ignore("E2E disabled. Set AFNEYGYM_RUN_E2E=1 and AFNEYGYM_BASE_URL=http://localhost:5171");
            return;
        }

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        if (_browser != null)
            await _browser.DisposeAsync();

        _playwright?.Dispose();
    }

    [Test]
    public async Task LoginPage_PasswordToggle_Works()
    {
        var baseUrl = GetBaseUrl();
        await using var context = await _browser!.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{baseUrl}/Account/Login");
        await page.WaitForSelectorAsync("#Password");

        var initialType = await page.GetAttributeAsync("#Password", "type");
        Assert.That(initialType, Is.EqualTo("password"));

        await page.ClickAsync("#togglePassword");
        var visibleType = await page.GetAttributeAsync("#Password", "type");
        Assert.That(visibleType, Is.EqualTo("text"));

        await page.ClickAsync("#togglePassword");
        var hiddenType = await page.GetAttributeAsync("#Password", "type");
        Assert.That(hiddenType, Is.EqualTo("password"));
    }

    [Test]
    public async Task HomePage_Loads_AndContainsLessonActionArea()
    {
        var baseUrl = GetBaseUrl();
        await using var context = await _browser!.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{baseUrl}/");
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var body = await page.TextContentAsync("body");
        Assert.That(body, Does.Contain("AFNEYGYM").IgnoreCase);
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ShowsErrorMessage()
    {
        var baseUrl = GetBaseUrl();
        await using var context = await _browser!.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{baseUrl}/Account/Login");
        await page.FillAsync("#Email", "notfound@afney.test");
        await page.FillAsync("#Password", "wrong-password");
        await page.ClickAsync("button[type='submit']");

        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        var body = await page.TextContentAsync("body");
        Assert.That(body, Does.Contain("E-posta veya şifre hatalı."));
    }

    [Test]
    public async Task ClassesPage_Loads_AndContainsScheduleHeader()
    {
        var baseUrl = GetBaseUrl();
        await using var context = await _browser!.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{baseUrl}/classes");
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var body = await page.TextContentAsync("body");
        Assert.That(body, Does.Contain("Ders Takvimi"));
    }

    [Test]
    public async Task MemberDashboard_AnonymousUser_IsRedirectedToLogin()
    {
        var baseUrl = GetBaseUrl();
        await using var context = await _browser!.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{baseUrl}/Member/Dashboard");
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        Assert.That(page.Url, Does.Contain("/Account/Login"));
    }

    private static bool IsE2EEnabled()
    {
        return string.Equals(Environment.GetEnvironmentVariable("AFNEYGYM_RUN_E2E"), "1", StringComparison.Ordinal);
    }

    private static string GetBaseUrl()
    {
        var baseUrl = Environment.GetEnvironmentVariable("AFNEYGYM_BASE_URL");
        return string.IsNullOrWhiteSpace(baseUrl) ? "http://localhost:5171" : baseUrl.TrimEnd('/');
    }
}

