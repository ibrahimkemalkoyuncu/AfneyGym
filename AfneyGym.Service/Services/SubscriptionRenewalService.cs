using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AfneyGym.Service.Services;

public class SubscriptionRenewalService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SubscriptionRenewalService> _logger;

    public SubscriptionRenewalService(AppDbContext context, ILogger<SubscriptionRenewalService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> ProcessDueSubscriptionsAsync(Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;

        var dueSubsQuery = _context.Subscriptions
            .Where(s => !s.IsDeleted && s.Status == SubscriptionStatus.Active && s.EndDate <= now);

        if (userId.HasValue && userId.Value != Guid.Empty)
            dueSubsQuery = dueSubsQuery.Where(s => s.UserId == userId.Value);

        var dueSubs = await dueSubsQuery
            .OrderBy(s => s.EndDate)
            .ToListAsync(cancellationToken);

        if (dueSubs.Count == 0)
            return 0;

        var expectedRefs = dueSubs
            .Select(s => BuildAutoRenewReference(s.Id, now))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingRefs = await _context.Payments
            .AsNoTracking()
            .Where(p => !p.IsDeleted && expectedRefs.Contains(p.ExternalReference))
            .Select(p => p.ExternalReference)
            .ToHashSetAsync(cancellationToken);

        var processedCount = 0;

        foreach (var sub in dueSubs)
        {
            if (!sub.AutoRenew)
            {
                sub.Status = SubscriptionStatus.Expired;
                sub.UpdatedAt = now;
                processedCount++;
                continue;
            }

            var autoRenewRef = BuildAutoRenewReference(sub.Id, now);
            if (existingRefs.Contains(autoRenewRef))
            {
                // Daha once ayni donem odeme olustuysa yalnizca sureyi normalize et.
                if (sub.EndDate <= now)
                {
                    sub.EndDate = now.AddMonths(1);
                    sub.LastRenewalDate ??= now;
                    sub.UpdatedAt = now;
                    processedCount++;
                }

                continue;
            }

            sub.EndDate = now.AddMonths(1);
            sub.LastRenewalDate = now;
            sub.UpdatedAt = now;

            _context.Payments.Add(new Payment
            {
                SubscriptionId = sub.Id,
                UserId = sub.UserId,
                Amount = ResolveMonthlyPrice(sub),
                Currency = "TRY",
                Provider = "AutoRenew",
                ExternalReference = autoRenewRef,
                Status = PaymentStatus.Completed,
                PaidAt = now,
                Note = "Otomatik yenileme islemi tamamlandi"
            });

            existingRefs.Add(autoRenewRef);
            processedCount++;
        }

        if (processedCount == 0)
            return 0;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return processedCount;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "AutoRenew kaydinda yaris kosulu yakalandi; tekrar isleme gerek yok.");
            _context.ChangeTracker.Clear();
            return 0;
        }
    }

    private static string BuildAutoRenewReference(Guid subscriptionId, DateTime now)
        => $"AUTO-{subscriptionId:N}-{now:yyyyMM}";

    private static decimal ResolveMonthlyPrice(Subscription sub)
    {
        return 1200m;
    }
}

