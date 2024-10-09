using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Middleware;
using vendtechext.BLL.Services;
using vendtechext.DAL.Models;
using signalrserver.HubConnection;
using FluentValidation;
using FluentValidation.AspNetCore;
using vendtechext.BLL.Validations;
using Hangfire;
using vendtechext.Helper;
using vendtechext.Helper.Configurations;
using vendtechext.BLL.Repository;
using vendtechext.BLL.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:56549", "https://vendtechsl.com")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

// Database Configuration
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Hangfire Configuration
builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHangfireServer();

// Configure strongly typed settings objects
builder.Services.Configure<ProviderInformation>(builder.Configuration.GetSection("ProviderInformation"));

// Add controllers and API behavior
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<IntegratorValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ElectricitySalesValidator>();

builder.Services.AddControllers();

// Identity configuration
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]??"")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

// Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
});

// SignalR Configuration
builder.Services.AddSignalR();

// Dependency Injection
builder.Services.AddScoped<IIntegratorService, IntegratorService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<IElectricitySalesService, ElectricitySalesService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<HttpRequestService>();
builder.Services.AddScoped<RequestExecutionContext>();
builder.Services.AddScoped<TransactionRepository>();

var app = builder.Build();

// Middleware Configuration
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Swagger Middleware
app.UseSwagger();
app.UseSwaggerUI();

// Enable CORS
app.UseCors("AllowSpecificOrigin");

// Map SignalR Hubs
app.MapHub<CustomersHub>("/customerHub");
app.MapHub<AdminHub>("/adminHub");

//Use Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    Authorization = new[] { new CustomAuthorizeFilter() }
});

// Use authentication and authorization
app.UseAuthentication(); // Ensure this is before UseAuthorization
app.UseAuthorization();

// Map Controllers
app.MapControllers();

app.Run();
