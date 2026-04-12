using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AfneyGym.Common.DTOs
{
    public record UserLoginDto
    {
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        public string Password { get; init; } = string.Empty;
    }
}
