using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AfneyGym.Service.Services;

namespace AfneyGym.Service.HostedServices;

/// <summary>
/// Otomatik üyelik yenileme ve kontrol işlemlerini günlük çalıştıran background servis.
/// - AutoRenew aktif ve süresi dolan üyelikleri 1 ay uzatır
/// - Süresi dolmuş üyelikleri Expired durumuna taşır
/// - Ödeme talebi bekleyen subscription'ları kontrol eder
/// </summary>
public class AutoRenewHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoRenewHostedService> _logger;
    private static readonly TimeSpan DailyCheckTime = new(2, 0, 0); // 02:00

    public AutoRenewHostedService(IServiceProvider serviceProvider, ILogger<AutoRenewHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoRenewHostedService başlatıldı");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var nextRun = now.Date.Add(DailyCheckTime);

                // Eğer günün işlemi yapılmışsa sonraki güne ertele
                if (now > nextRun)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                _logger.LogInformation($"Sonraki otomatik yenileme kontrolü: {nextRun:yyyy-MM-dd HH:mm:ss}");

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    await ProcessAutoRenewalsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("AutoRenewHostedService iptal edildi");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutoRenewHostedService'de hata oluştu");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task ProcessAutoRenewalsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var renewalService = scope.ServiceProvider.GetRequiredService<SubscriptionRenewalService>();

        try
        {
            _logger.LogInformation("Otomatik yenileme işlemi başladı");
            var processedCount = await renewalService.ProcessDueSubscriptionsAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("Otomatik yenileme işlemi tamamlandı. {ProcessedCount} subscription işlendi", processedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Otomatik yenileme işlemi sırasında hata oluştu");
        }
    }
}

