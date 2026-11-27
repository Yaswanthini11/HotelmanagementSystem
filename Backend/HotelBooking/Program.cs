
using HotelBooking.Data;
using HotelBooking.Models;
using HotelBooking.Repositories;
using HotelBooking.Repositories.Implementations;
using HotelBooking.Repositories.Interfaces;
using HotelBooking.Services;
using HotelBooking.Services.Implementations;
using HotelBooking.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------
// 1. Add Controllers
// --------------------------------------

builder.Services.AddControllers();

// --------------------------------------
// 2. Configure SQL Server Database
// --------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// --------------------------------------
// 3. Configure ASP.NET Core Identity
// --------------------------------------

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// --------------------------------------
// 4. Configure JWT Authentication
// --------------------------------------

var jwtKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is missing.");

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is missing.");

var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is missing.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must contain at least 32 bytes.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// --------------------------------------
// 5. Configure Authorization
// --------------------------------------

builder.Services.AddAuthorization();

// --------------------------------------
// 6. Configure CORS for Angular
// --------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --------------------------------------
// 7. Register Repositories
// --------------------------------------

// Hotel repositories
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IAmenityRepository, AmenityRepository>();

// Room and booking repositories
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IRoomInventoryRepository, RoomInventoryRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// --------------------------------------
// 8. Register Services
// --------------------------------------

// Hotel and amenity services
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();

// Room and booking services
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IRoomInventoryService, RoomInventoryService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Authentication
builder.Services.AddScoped<IAuthService, AuthService>();

// --------------------------------------
// 9. Configure OpenAPI
// --------------------------------------

builder.Services.AddOpenApi();

var app = builder.Build();

// --------------------------------------
// 10. Seed Admin Role and Initial Admin
// --------------------------------------

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    const string adminRole = "Admin";

    // Ensure Admin role exists.
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        var roleResult = await roleManager.CreateAsync(
            new IdentityRole(adminRole));

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to create Admin role: " +
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }

    // Read Admin bootstrap configuration from User Secrets.
    var adminEmail = builder.Configuration["AdminBootstrap:Email"];
    var adminPassword = builder.Configuration["AdminBootstrap:Password"];
    var adminFullName = builder.Configuration["AdminBootstrap:FullName"];

    if (string.IsNullOrWhiteSpace(adminEmail) ||
        string.IsNullOrWhiteSpace(adminPassword) ||
        string.IsNullOrWhiteSpace(adminFullName))
    {
        throw new InvalidOperationException(
            "Admin bootstrap configuration is incomplete. " +
            "Configure AdminBootstrap:Email, Password and FullName using User Secrets.");
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        // Create the initial Admin account.
        adminUser = new ApplicationUser
        {
            FullName = adminFullName.Trim(),
            Email = adminEmail.Trim(),
            UserName = adminEmail.Trim(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(
            adminUser,
            adminPassword);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to create initial Admin: " +
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        var addRoleResult = await userManager.AddToRoleAsync(
            adminUser,
            adminRole);

        if (!addRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(adminUser);

            throw new InvalidOperationException(
                "Failed to assign Admin role: " +
                string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
        }

        app.Logger.LogInformation(
            "Initial Admin account created successfully.");
    }
    else
    {
        var isAdmin = await userManager.IsInRoleAsync(
            adminUser,
            adminRole);

        if (!isAdmin)
        {
            throw new InvalidOperationException(
                "The configured Admin email already belongs to a non-Admin account. " +
                "Choose a different AdminBootstrap email.");
        }

        app.Logger.LogInformation(
            "Existing Admin account verified.");
    }
}

// --------------------------------------
// 11. Configure HTTP Pipeline
// --------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AngularPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
