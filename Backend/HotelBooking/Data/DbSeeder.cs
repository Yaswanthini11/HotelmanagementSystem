
using HotelBooking.Models;
using Microsoft.AspNetCore.Identity;

namespace HotelBooking.Data;

public static class DbSeeder
{
	public static async Task SeedAsync(
		IServiceProvider services,
		IConfiguration configuration)
	{
		using var scope = services.CreateScope();

		var roleManager = scope.ServiceProvider
			.GetRequiredService<RoleManager<IdentityRole>>();

		var userManager = scope.ServiceProvider
			.GetRequiredService<UserManager<ApplicationUser>>();

		// 1. Create roles
		string[] roles = { "Admin", "Customer" };

		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
			{
				var roleResult = await roleManager.CreateAsync(
					new IdentityRole(role));

				if (!roleResult.Succeeded)
				{
					throw new Exception(
						$"Could not create role {role}: " +
						string.Join(", ", roleResult.Errors.Select(e => e.Description)));
				}
			}
		}

		// 2. Read Admin credentials from configuration
		var email = configuration["Admin:Email"];
		var password = configuration["Admin:Password"];

		if (string.IsNullOrWhiteSpace(email) ||
			string.IsNullOrWhiteSpace(password))
		{
			throw new InvalidOperationException(
				"Admin email or password is missing from configuration.");
		}

		// 3. Check whether Admin already exists
		var admin = await userManager.FindByEmailAsync(email);

		if (admin == null)
		{
			admin = new ApplicationUser
			{
				FullName = "Hotel Administrator",
				UserName = email,
				Email = email,
				EmailConfirmed = true
			};

			var createResult = await userManager.CreateAsync(
				admin, password);

			if (!createResult.Succeeded)
			{
				throw new Exception(
					"Could not create Admin: " +
					string.Join(", ", createResult.Errors.Select(e => e.Description)));
			}
		}

		// 4. Assign Admin role
		if (!await userManager.IsInRoleAsync(admin, "Admin"))
		{
			var roleResult = await userManager.AddToRoleAsync(
				admin, "Admin");

			if (!roleResult.Succeeded)
			{
				throw new Exception(
					"Could not assign Admin role: " +
					string.Join(", ", roleResult.Errors.Select(e => e.Description)));
			}
		}
	}
}
