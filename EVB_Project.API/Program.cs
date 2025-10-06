using EVB_Project.API.Extensions;
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

var builder = WebApplication.CreateBuilder(args);

// a) Connection string: ưu tiên ConnectionStrings__DefaultConnection; fallback DATABASE_URL (postgres://)
var envConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
if (!string.IsNullOrWhiteSpace(envConn))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] = envConn;
}
else
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var npgsqlConn =
            $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
            $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=True";
        builder.Configuration["ConnectionStrings:DefaultConnection"] = npgsqlConn;
    }
}

// b) JWT: hỗ trợ cả 2 kiểu key: Jwt:Key (appsettings hiện tại) hoặc JwtSettings:SecretKey (ENV kiểu cũ)
string jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? builder.Configuration["JwtSettings:SecretKey"]
    ?? ""; // sẽ check sau để báo lỗi rõ ràng

string jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? builder.Configuration["JwtSettings:Issuer"]
    ?? "EVB-API";

string jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? builder.Configuration["JwtSettings:Audience"]
    ?? "EVB-CLIENT";

// --- 2) ĐĂNG KÝ DỊCH VỤ ---
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer' + khoảng trắng + token"
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
builder.Services.AddGlobalExceptionHandling();
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

// Add DbContext
builder.Services.AddDbContext<EVBatteryTradingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Mapster mappings
MapsterConfig.RegisterMappings();

// JWT từ appsettings
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// ---- Health endpoint cho Render ----
app.MapGet("/health", () => Results.Ok("OK"));

// ---- Auto-migrate on startup (only if ApplyMigrations=true) ----
if (builder.Configuration.GetValue<bool>("ApplyMigrations", false))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EVBatteryTradingContext>();

    const int maxRetries = 5;
    int retries = maxRetries;

    while (true)
    {
        try
        {
            db.Database.Migrate();
            Console.WriteLine("✅ Database migrated successfully.");
            break;
        }
        catch (Exception ex) when (retries-- > 0)
        {
            Console.WriteLine($"⚠️ Migration failed (retries left: {retries}). Error: {ex.Message}");
            Thread.Sleep(5000); // 5s chờ DB Render sẵn sàng
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Migration aborted after {maxRetries} attempts. Error: {ex.Message}");
            break;
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// đặt ExceptionMiddleware sớm (trước MapControllers)
app.UseGlobalExceptionHandling();

app.MapControllers();

app.Run();
