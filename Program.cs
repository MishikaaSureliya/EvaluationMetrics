using EvolutionMetrics.Data;
using EvolutionMetrics.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ✅ Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

// ✅ Services
builder.Services.AddSingleton<RainfallService>();
builder.Services.AddSingleton<CaloriesService>();
builder.Services.AddSingleton<LaptopService>();

// ✅ JWT Service
builder.Services.AddScoped<JwtService>();

// ✅ Controllers
builder.Services.AddControllersWithViews();

// ✅ JWT Authentication
var key = Encoding.UTF8.GetBytes("THIS_IS_MY_SUPER_SECRET_KEY_123456789");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer YOUR_TOKEN"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

var app = builder.Build();

// ✅ Load ML Models
var rainfallService = app.Services.GetRequiredService<RainfallService>();
rainfallService.LoadOrTrain("Dataset/rainfall.csv");

var caloriesService = app.Services.GetRequiredService<CaloriesService>();
caloriesService.LoadOrTrain("Dataset/calories.csv");

var laptopService = app.Services.GetRequiredService<LaptopService>();
laptopService.LoadOrTrain("Dataset/laptop.csv");

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.UseHttpsRedirection();

// ✅ IMPORTANT ORDER
app.UseAuthentication();   // 🔥 MUST COME BEFORE AUTHORIZATION
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
);

app.Run();