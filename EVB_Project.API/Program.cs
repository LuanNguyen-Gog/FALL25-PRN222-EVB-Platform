using EVB_Project.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories.DBContext;
using Repositories.Repository;
using Services;
using Services.Implement;
using Services.Interface;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// đăng ký middleware qua DI
//Dùng middleware để bắt lỗi toàn cục tránh việc phải try catch ở từng action(controller)
builder.Services.AddTransient<GlobalExceptionMiddleware>();
//Add scoped services
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<BatteryRepository>();
builder.Services.AddScoped<IBatteryService, BatteryService>();
builder.Services.AddScoped<ListingRepository>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<TokenRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

// Configure JSON options to use string enums
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Add DbContext
builder.Services.AddDbContext<EVBatteryTradingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DATABASE_URL"], npg =>
    {
        npg.CommandTimeout(30); // 30s
    }));

// Register Mapster mappings
MapsterConfig.RegisterMappings();

// JWT configuration - use only appsettings.json values
builder.Services.AddAuthentication(op => 
{
    op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    op.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["key"]!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

// ---- Auto-migrate on startup (only if ApplyMigrations=true) ----
if (builder.Configuration.GetValue<bool>("ApplyMigrations", false))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EVBatteryTradingContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EVB_Project.API v1");
        c.RoutePrefix = "swagger"; // Serve the Swagger UI at the app's root
    });
}

app.UseCors("DevCors");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Bắt lỗi sau Auth/Author để không “đổi màu” 401/403 chính thống
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapGet("/health", () => Results.Ok("OK")).AllowAnonymous();
app.MapControllers();

app.Run();
