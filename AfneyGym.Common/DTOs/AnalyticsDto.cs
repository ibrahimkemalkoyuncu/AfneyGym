namespace AfneyGym.Common.DTOs;

public class AnalyticsDto
{
    // Üyelik metrikler
    public int TotalMembers { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int PendingApprovals { get; set; }
    public int SubscriptionRenewalRate { get; set; } // %

    // Ders metrikler
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public decimal AverageLessonAttendance { get; set; } // %
    public int NoShowCount { get; set; }

    // Ödeme metrikler
    public decimal TotalRevenue { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public decimal AverageSubscriptionPrice { get; set; }

    // Eğitmen metrikler
    public int TotalTrainers { get; set; }
    public int ActiveTrainers { get; set; }

    // A/B Test
    public int HeroVariantACount { get; set; }
    public int HeroVariantBCount { get; set; }

    // Tarihsel veriler (son 7 gün)
    public List<DailyMetricDto> Last7DaysMetrics { get; set; } = new();
}

public class DailyMetricDto
{
    public DateTime Date { get; set; }
    public int NewSubscriptions { get; set; }
    public int LessonsHeld { get; set; }
    public decimal DailyRevenue { get; set; }
    public int Registrations { get; set; }
}

