using System.ComponentModel.DataAnnotations;
using AfneyGym.Common.Enums; // Domain yerine Common içindeki Enum kullanılıyor

namespace AfneyGym.Common.DTOs;

public class UserUpdateDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçersiz e-posta formatı.")]
    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; } // CS0246 Çözüldü

    public Guid? GymId { get; set; }
}