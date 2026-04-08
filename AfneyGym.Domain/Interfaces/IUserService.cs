//using AfneyGym.Domain.Entities;
//using AfneyGym.Common.DTOs; // Eklendi


//namespace AfneyGym.Domain.Interfaces;

//public interface IUserService
//{
//    Task<User?> GetByEmailAsync(string email);
//    // UserRegisterDto tipini kabul edecek şekilde güncellendi
//    Task<bool> RegisterAsync(UserRegisterDto registerDto);
//    // UserLoginDto tipini kabul edecek şekilde güncellendi
//    Task<User?> LoginAsync(UserLoginDto loginDto);
//}


using AfneyGym.Domain.Entities;
using AfneyGym.Common.DTOs;

namespace AfneyGym.Domain.Interfaces;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email);

    // Sorumluluk Ayrımı: Domain entity yerine DTO kullanımı güvenliği artırır.
    Task<bool> RegisterAsync(UserRegisterDto registerDto);

    Task<User?> LoginAsync(UserLoginDto loginDto);
}