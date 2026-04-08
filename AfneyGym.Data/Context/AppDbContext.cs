using AfneyGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AfneyGym.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Gym> Gyms { get; set; }
    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<LessonAttendee> LessonAttendees { get; set; } // YENİ EKLENDİ

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
        });

        // LessonAttendee Konfigürasyonu (YENİ)
        modelBuilder.Entity<LessonAttendee>(entity =>
        {
            entity.HasKey(la => la.Id);
            // Unique Constraint: Bir üye bir derse sadece bir kez kayıt olabilir
            entity.HasIndex(la => new { la.LessonId, la.UserId }).IsUnique();
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(l => l.Trainer).WithMany(t => t.Lessons).HasForeignKey(l => l.TrainerId);
        });
    }
}