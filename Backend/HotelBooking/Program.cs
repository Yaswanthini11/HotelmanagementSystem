
using System.Text;
using System.Security.Claims;

using HotelBooking.Data;
using HotelBooking.Models;

// Repository namespaces
using HotelBooking.Repositories;
using HotelBooking.Repositories.Interfaces;
using HotelBooking.Repositories.Implementations;

// Service namespaces
using HotelBooking.Services;
using HotelBooking.Services.Interfaces;
using HotelBooking.Services.Implementations;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IAmenityRepository, AmenityRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IRoomInventoryRepository, RoomInventoryRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
// --------------------------------------
// 8. Register Services
// --------------------------------------

builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IRoomInventoryService, RoomInventoryService>();
builder.Services.AddScoped<IBookingService, BookingService>();
// --------------------------------------
// 9. Configure OpenAPI
// --------------------------------------

builder.Services.AddOpenApi();

var app = builder.Build();

// --------------------------------------
// 10. Seed Admin and Roles
// --------------------------------------

await DbSeeder.SeedAsync(app.Services, app.Configuration);

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
