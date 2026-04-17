using AfneyGym.Data.Context;
using AfneyGym.Domain.Interfaces;
using AfneyGym.Service.Hubs;
using AfneyGym.Service.Services;
using AfneyGym.Common.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog; // Eklendi

var builder = WebApplication.CreateBuilder(args);

// --- SERILOG YAPILANDIRMASI (YENİ) ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // Serilog'u ana log sağlayıcısı yap
// --------------------------------------

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<IyzicoSettings>(builder.Configuration.GetSection("IyzicoSettings"));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IIyzicoGateway, IyzicoGatewayService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITrainerService, TrainerService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IMemberLifecycleService, MemberLifecycleService>();
builder.Services.AddScoped<SubscriptionRenewalService>();

builder.Services.AddHostedService<AfneyGym.Service.HostedServices.AutoRenewHostedService>();
builder.Services.AddHostedService<AfneyGym.Service.HostedServices.LessonReminderHostedService>();
builder.Services.AddHostedService<AfneyGym.Service.HostedServices.MemberLifecycleHostedService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.Name = "AfneyGymAuthCookie";
    });

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

var app = builder.Build();

// REGRESSION CHECK: Hata ayıklama ve statik dosyalar korunuyor
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSerilogRequestLogging(); // HTTP isteklerini logla (YENİ)

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");
app.MapGet("/ready", () => Results.Ok(new { status = "ready", timestamp = DateTime.UtcNow }));

try
{
    Log.Information("AfneyGym Uygulaması Başlatılıyor...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama beklenmedik bir şekilde sonlandı.");
}
finally
{
    Log.CloseAndFlush();
}