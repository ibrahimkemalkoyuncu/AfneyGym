using AfneyGym.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AfneyGym.Service.HostedServices;

/// <summary>
/// Her gece 02:15'te lifecycle statularini gunceller ve gerekli hatirlatma e-postalarini gonderir.
/// </summary>
public class MemberLifecycleHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MemberLifecycleHostedService> _logger;
    private static readonly TimeSpan DailyRunTime = new(2, 15, 0);

    public MemberLifecycleHostedService(IServiceProvider serviceProvider, ILogger<MemberLifecycleHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MemberLifecycleHostedService baslatildi");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var nextRun = now.Date.Add(DailyRunTime);
                if (now > nextRun)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                using var scope = _serviceProvider.CreateScope();
                var lifecycleService = scope.ServiceProvider.GetRequiredService<IMemberLifecycleService>();

                var updatedCount = await lifecycleService.UpdateAllMembersLifecycleAsync();
                await lifecycleService.SendAtRiskRemindersAsync();
                await lifecycleService.SendRenewalRemindersAsync();

                _logger.LogInformation("Member lifecycle nightly job tamamlandi. Guncellenen uye sayisi: {Count}", updatedCount);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("MemberLifecycleHostedService iptal edildi");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MemberLifecycleHostedService calisirken hata olustu");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}

