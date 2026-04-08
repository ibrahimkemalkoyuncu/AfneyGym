namespace AfneyGym.Domain.Entities;

public class Trainer : BaseEntity
{
    public required string FullName { get; set; }
    public string? Specialty { get; set; } // Örn: Pilates, Crossfit
    public string? Bio { get; set; }
    public string? ImageUrl { get; set; } // Figma tasarımındaki profil fotoğrafları için
    public virtual ICollection<Lesson>? Lessons { get; set; }
}