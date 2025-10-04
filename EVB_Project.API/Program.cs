using EVB_Project.API.Extensions;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Repository;
using Services;
using Services.Implement;
using Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Add DbContext
builder.Services.AddDbContext<EVBatteryTradingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Mapster mappings
MapsterConfig.RegisterMappings();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// đặt ExceptionMiddleware sớm (trước MapControllers)
app.UseGlobalExceptionHandling();

app.MapControllers();

app.Run();
