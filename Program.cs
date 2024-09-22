using Hangfire;
using System.Text;
using NReco.Logging.File;
using ImageProcessor.Data;
using ImageProcessor.Enums;
using ImageProcessor.Jobs;
using ImageProcessor.Models;
using ImageProcessor.Seeder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ImageProcessor.Services.Contracts;
using ImageProcessor.Services.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

//Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Default", builder => builder.WithOrigins("http://localhost:3000", "https://dev.cgcsa.invokesolutions.co.za")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

// Add services to the container.
builder.Services.AddScoped<RoleTableSeeder>();
builder.Services.AddScoped<UserTableSeeder>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IimageService, ImageService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<ICodeGenerator, CodeGenerator>();
builder.Services.AddHttpClient();
builder.Services.AddHangfireServer();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add Hang fire
builder.Services.AddHangfire(options =>
{
    options.UseSqlServerStorage(builder.Configuration.GetConnectionString(ConfigConst.DefaultConnection.ToString()));
});

//Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    var securityKey = builder.Configuration.GetSection(ConfigConst.Authentication.ToString()).GetSection(ConfigConst.SecurityKey.ToString()).Value;

    if (securityKey != null)
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey)),
            ValidIssuer = ConfigConst.ImageProcessorApi.ToString(),
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    }
});

//Add Authorization
builder.Services.AddAuthorizationBuilder();

//Configure DBContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString(ConfigConst.DefaultConnection.ToString()));
});

builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();



//Configure logging
builder.Services.AddLogging(logBuilder =>
{
    logBuilder.AddFile("C:\\Users\\pauli\\OneDrive\\Pictures\\Saved Pictures/log.txt", append: true);
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseCors("Default");

using (var serviceScope = app.Services.CreateScope())
{
    var roleSeeder = serviceScope.ServiceProvider.GetRequiredService<RoleTableSeeder>();
    roleSeeder.SeedRolesAsync().Wait();

    var userSeeder = serviceScope.ServiceProvider.GetRequiredService<UserTableSeeder>();
    userSeeder.SeedAdministratorsAsync().Wait();
}

RecurringJob.AddOrUpdate<ImageProcessJob>(job => job.ProcessImageQueue(), "*/5 * * * * *");

app.Run();
