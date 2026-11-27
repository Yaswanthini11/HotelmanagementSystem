
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelBooking.DTOs.Auth;
using HotelBooking.Models;
using HotelBooking.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace HotelBooking.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException(
                "Password and confirm password do not match.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(
            user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ", result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(errors);
        }

        const string customerRole = "Customer";

        if (!await _roleManager.RoleExistsAsync(customerRole))
        {
            var roleResult = await _roleManager.CreateAsync(
                new IdentityRole(customerRole));

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                var errors = string.Join(
                    ", ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }
        }

        var roleAssignment = await _userManager.AddToRoleAsync(
            user, customerRole);

        if (!roleAssignment.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = string.Join(
                ", ",
                roleAssignment.Errors.Select(e => e.Description));

            throw new InvalidOperationException(errors);
        }

        return await GenerateAuthResponseAsync(user, customerRole);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(
            user, request.Password);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Count == 0)
        {
            throw new UnauthorizedAccessException(
                "User does not have an assigned role.");
        }

        return await GenerateAuthResponseAsync(user, roles[0]);
    }

    public async Task<CurrentUserResponseDto?> GetCurrentUserAsync(
        string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new CurrentUserResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty
        };
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(
        ApplicationUser user,
        string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey is missing.");

        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        var expiryMinutes = int.TryParse(
            jwtSettings["ExpiryMinutes"], out var minutes)
            ? minutes
            : 60;

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return new AuthResponseDto
        {
            Token = tokenString,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = role
        };
    }
}
