using AfneyGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AfneyGym.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<Gym> Gyms { get; set; } = null!;
    public DbSet<Trainer> Trainers { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<LessonAttendee> LessonAttendees { get; set; } = null!; // YENİ EKLENDİ
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<HeroVariantExposure> HeroVariantExposures { get; set; } = null!;

    // --- PHASE 1: ÜYE LİFESİKLE TAKIBI ---
    public DbSet<UserBodyMetric> UserBodyMetrics { get; set; } = null!;
    public DbSet<UserGoal> UserGoals { get; set; } = null!;
    public DbSet<GymCheckIn> GymCheckIns { get; set; } = null!;
    public DbSet<UserLifecycleStatus> UserLifecycleStatuses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Konfigürasyonu
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Subscription Konfigürasyonu
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.AutoRenew, e.EndDate });
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(8);
            entity.Property(e => e.Provider).HasMaxLength(32);
            entity.Property(e => e.ExternalReference).HasMaxLength(64);
            entity.HasIndex(e => e.ExternalReference).IsUnique();
            entity.HasIndex(e => e.Status);

            entity.HasOne(p => p.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(64);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.IssuedAt);

            entity.HasOne(i => i.Payment)
                .WithOne(p => p.Invoice)
                .HasForeignKey<Invoice>(i => i.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.User)
                .WithMany(u => u.Invoices)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HeroVariantExposure>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VisitorId).HasMaxLength(64);
            entity.Property(e => e.Variant).HasMaxLength(1);
            entity.HasIndex(e => e.VisitorId).IsUnique();
            entity.HasIndex(e => e.Variant);
        });

        // LessonAttendee Konfigürasyonu (YENİ)
        modelBuilder.Entity<LessonAttendee>(entity =>
        {
            entity.HasKey(la => la.Id);
            // Unique Constraint: Bir üye bir derse sadece bir kez kayıt olabilir
            entity.HasIndex(la => new { la.LessonId, la.UserId }).IsUnique();
            entity.HasIndex(la => la.ReminderSentAt);
        });

        // --- PHASE 1: ÜYE LİFESİKLE KONFIGÜRASYONLARI ---
        modelBuilder.Entity<UserBodyMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.MeasurementDate }).IsUnique();
            entity.Property(e => e.Weight).HasPrecision(5, 2);
            entity.Property(e => e.BodyFatPercentage).HasPrecision(5, 2);
            entity.Property(e => e.MuscleMass).HasPrecision(5, 2);
            entity.Property(e => e.BMI).HasPrecision(5, 2);
            entity.Property(e => e.ChestCircumference).HasPrecision(6, 2);
            entity.Property(e => e.WaistCircumference).HasPrecision(6, 2);
            entity.Property(e => e.HipCircumference).HasPrecision(6, 2);
            entity.Property(e => e.ArmCircumference).HasPrecision(6, 2);
            entity.HasOne(e => e.User)
                .WithMany(u => u.BodyMetrics)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserGoal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.Property(e => e.StartValue).HasPrecision(10, 2);
            entity.Property(e => e.TargetValue).HasPrecision(10, 2);
            entity.Property(e => e.CurrentValue).HasPrecision(10, 2);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GymCheckIn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.CheckInTime });
            entity.HasOne(e => e.User)
                .WithMany(u => u.CheckIns)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserLifecycleStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.CurrentStage }).IsUnique();
            entity.HasIndex(e => e.RiskScore);
            entity.HasOne(e => e.User)
                .WithOne(u => u.LifecycleStatus)
                .HasForeignKey<UserLifecycleStatus>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(l => l.Trainer).WithMany(t => t.Lessons).HasForeignKey(l => l.TrainerId);
        });
    }
}