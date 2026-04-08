//using AfneyGym.Data.Context;
//using AfneyGym.Domain.Entities;
//using AfneyGym.Domain.Interfaces;
//using AfneyGym.Common.DTOs; // Eklendi
//using Microsoft.EntityFrameworkCore;
//using BC = BCrypt.Net.BCrypt;

//namespace AfneyGym.Service.Services;

//public class UserService : IUserService
//{
//    private readonly AppDbContext _context;

//    public UserService(AppDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<User?> GetByEmailAsync(string email)
//    {
//        // Email standardizasyonu: Küçük harfe çevrilerek aranır
//        string normalizedEmail = email.Trim().ToLower();
//        return await _context.Users
//            .Include(u => u.Subscriptions)
//            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted);
//    }

//    public async Task<bool> RegisterAsync(UserRegisterDto registerDto)
//    {
//        // 1. E-posta kontrolü (Normalize edilmiş haliyle)
//        var userExists = await GetByEmailAsync(registerDto.Email);
//        if (userExists != null) return false;

//        // 2. Manuel Mapping: DTO -> Entity (Değişiklik Burada)
//        var user = new User
//        {
//            FirstName = registerDto.FirstName,
//            LastName = registerDto.LastName,
//            Email = registerDto.Email.Trim().ToLower(),
//            PasswordHash = BC.HashPassword(registerDto.Password, workFactor: 12),
//            Role = UserRole.Member // Varsayılan rol: Üye
//        };

//        await _context.Users.AddAsync(user);
//        await _context.SaveChangesAsync();
//        return true;
//    }

//    public async Task<User?> LoginAsync(UserLoginDto loginDto)
//    {
//        var user = await GetByEmailAsync(loginDto.Email);
//        if (user == null) return null;

//        // BCrypt doğrulaması
//        bool isValid = BC.Verify(loginDto.Password, user.PasswordHash);

//        return isValid ? user : null;
//    }
//}



using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using AfneyGym.Common.DTOs;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace AfneyGym.Service.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        // Analiz: Trim ve ToLower ile case-sensitivity sorunları önlenir.
        string normalizedEmail = email.Trim().ToLower();

        return await _context.Users
            .Include(u => u.Subscriptions) // CS1061 çözümü için User entity'sinde tanımlandı.
            .Include(u => u.Gym)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted);
    }

    public async Task<bool> RegisterAsync(UserRegisterDto registerDto)
    {
        // 1. E-posta mükerrerlik kontrolü
        var userExists = await GetByEmailAsync(registerDto.Email);
        if (userExists != null) return false;

        // 2. Mapping: DTO -> Entity (Security: BCrypt workFactor 12 ideal dengedir)
        var user = new User
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email.Trim().ToLower(),
            PasswordHash = BC.HashPassword(registerDto.Password, workFactor: 12),
            Role = UserRole.Member, // CS0103 çözümü: Domain.Entities içindeki Enum kullanıldı.
            CreatedAt = DateTime.Now
        };

        await _context.Users.AddAsync(user);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<User?> LoginAsync(UserLoginDto loginDto)
    {
        var user = await GetByEmailAsync(loginDto.Email);
        if (user == null) return null;

        // BCrypt doğrulaması: Plain password vs Hashed password
        bool isValid = BC.Verify(loginDto.Password, user.PasswordHash);

        return isValid ? user : null;
    }
}