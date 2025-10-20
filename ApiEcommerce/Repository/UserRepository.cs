using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository;

public class UserRepository : IUserRepository
{
    public readonly ApplicationDbContext _db;
    private string? secretKey;

    public UserRepository(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
    }

    public User? GetUser(int id)
    {
        return _db.Users.FirstOrDefault(u => u.Id == id);
    }

    public ICollection<User> GetUsers()
    {
        return _db.Users.OrderBy(u => u.Name).ToList();
    }

    public bool IsUniqueUser(string username)
    {
        return !_db.Users.Any(u => u.Username.ToLower().Trim() == username.ToLower().Trim());
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Username))
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Username is required",
            };
        }

        var userFromDb = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username.ToLower().Trim() == userLoginDto.Username.ToLower().Trim()
        );

        if (userFromDb == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Username not found",
            };
        }

        if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, userFromDb.Password))
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Credentials are incorrect",
            };
        }

        // JWT
        var tokenHandler = new JwtSecurityTokenHandler();

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Secret key is null or empty.");
        }

        var key = Encoding.UTF8.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new[]
                {
                    new Claim("id", userFromDb.Id.ToString()),
                    new Claim("username", userFromDb.Username),
                    new Claim(ClaimTypes.Role, userFromDb.Role ?? string.Empty),
                }
            ),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new UserLoginResponseDto()
        {
            Token = tokenHandler.WriteToken(token),
            User = new UserRegisterDto()
            {
                Username = userFromDb.Username,
                Name = userFromDb.Name,
                Role = userFromDb.Role,
                Password = userFromDb.Password ?? string.Empty,
            },
            Message = "Authentication successful",
        };
    }

    public async Task<User> Register(CreateUserDto createUserDto)
    {
        var encryptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        var user = new User()
        {
            Username = createUserDto.Username ?? "No username",
            Name = createUserDto.Name,
            Role = createUserDto.Role,
            Password = encryptedPassword,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}
