using System.ComponentModel.DataAnnotations;

namespace AfneyGym.Common.DTOs;

// Kayıt sırasında kullanılan model
public record UserRegisterDto
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [StringLength(50)]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [StringLength(50)]
    public string LastName { get; init; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçersiz e-posta formatı.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; init; } = string.Empty;

    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; init; } = string.Empty;
}
