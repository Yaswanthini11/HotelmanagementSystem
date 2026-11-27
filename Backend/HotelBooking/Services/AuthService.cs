
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using HotelBooking.Data;
using HotelBooking.DTOs.Auth;
using HotelBooking.Models;
using HotelBooking.Services.Interfaces;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

	// REGISTER
	public async Task<AuthResponseDto> RegisterAsync(
		RegisterRequestDto request)
	{
		var existingUser = await _userManager.FindByEmailAsync(request.Email);

		if (existingUser != null)
			throw new InvalidOperationException("Email is already registered.");

		// Ensure Customer role exists
		if (!await _roleManager.RoleExistsAsync("Customer"))
		{
			var roleResult = await _roleManager.CreateAsync(
				new IdentityRole("Customer"));

			if (!roleResult.Succeeded)
				throw new InvalidOperationException(
					string.Join(", ", roleResult.Errors.Select(e => e.Description)));
		}

		var user = new ApplicationUser
		{
			UserName = request.Email,
			Email = request.Email,
			FullName = request.FullName
		};

		var result = await _userManager.CreateAsync(user, request.Password);

		if (!result.Succeeded)
		{
			throw new InvalidOperationException(
				string.Join(", ", result.Errors.Select(e => e.Description)));
		}

		var addRoleResult = await _userManager.AddToRoleAsync(user, "Customer");

		if (!addRoleResult.Succeeded)
		{
			await _userManager.DeleteAsync(user);

			throw new InvalidOperationException(
				string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
		}

		return await GenerateAuthResponseAsync(user, "Customer");
	}

	// LOGIN
	public async Task<AuthResponseDto> LoginAsync(
		LoginRequestDto request)
	{
		var user = await _userManager.FindByEmailAsync(request.Email);

		if (user == null)
			throw new UnauthorizedAccessException("Invalid email or password.");

		var passwordValid = await _userManager.CheckPasswordAsync(
			user, request.Password);

		if (!passwordValid)
			throw new UnauthorizedAccessException("Invalid email or password.");

		var roles = await _userManager.GetRolesAsync(user);

		var role = roles.FirstOrDefault();

		if (string.IsNullOrEmpty(role))
			throw new UnauthorizedAccessException("User has no assigned role.");

		return await GenerateAuthResponseAsync(user, role);
	}

	// GET CURRENT USER
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

	// GENERATE JWT TOKEN
	private async Task<AuthResponseDto> GenerateAuthResponseAsync(
		ApplicationUser user,
		string role)
	{
		var secretKey = _configuration["JwtSettings:SecretKey"]
			?? throw new InvalidOperationException("JWT SecretKey is missing.");

		var issuer = _configuration["JwtSettings:Issuer"]
			?? throw new InvalidOperationException("JWT Issuer is missing.");

		var audience = _configuration["JwtSettings:Audience"]
			?? throw new InvalidOperationException("JWT Audience is missing.");

		var expiryMinutes = _configuration.GetValue<int?>(
			"JwtSettings:ExpiryMinutes") ?? 60;

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
			signingCredentials: credentials);

		var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

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
