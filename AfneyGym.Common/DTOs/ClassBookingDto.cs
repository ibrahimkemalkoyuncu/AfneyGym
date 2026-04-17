namespace AfneyGym.Common.DTOs;

/// <summary>
/// Ders oluşturma/güncelleme için DTO
/// </summary>
public class ClassBookingCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public Guid TrainerId { get; set; }
}

/// <summary>
/// Ders listesi için DTO (Capacity info ile)
/// </summary>
public class ClassBookingListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public int RegisteredCount { get; set; }
    public int AvailableSpots => Capacity - RegisteredCount;
    public bool IsFull => RegisteredCount >= Capacity;
    public string TrainerName { get; set; } = string.Empty;
    public string DurationText => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
    public string TimeUntilStart
    {
        get
        {
            var diff = StartTime - DateTime.Now;
            if (diff.TotalMinutes < 0) return "Başladı";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} dakika kaldı";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} saat kaldı";
            return $"{(int)diff.TotalDays} gün kaldı";
        }
    }
}

/// <summary>
/// Ders detay (katılımcılar ile)
/// </summary>
public class ClassBookingDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public int RegisteredCount { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public List<ClassAttendeeDto> Attendees { get; set; } = new();
}

/// <summary>
/// Ders katılımcısı
/// </summary>
public class ClassAttendeeDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public bool IsAttended { get; set; }
}

