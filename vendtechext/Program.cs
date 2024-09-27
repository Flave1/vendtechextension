using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Configurations;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Middleware;
using vendtechext.BLL.Services;
using vendtechext.DAL.Models;
using signalrserver.HubConnection;
using FluentValidation;
using FluentValidation.AspNetCore;
using vendtechext.BLL.Validations;
using Hangfire;
using vendtechext.Hangfire;
using vendtechext.BLL;

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
                   .AllowCredentials();  // Explicitly allow specific origins
                   //.SetIsOriginAllowed((host) => true);
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
builder.Services.Configure<RTSInformation>(builder.Configuration.GetSection("RTSInformation"));

// Add controllers and API behavior
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<IntegratorValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ElectricitySalesValidator>();

builder.Services.AddControllers();

// Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
});

// SignalR Configuration
builder.Services.AddSignalR();

// Dependency Injection
builder.Services.AddScoped<IB2bAccountService, B2bAccountService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IElectricitySalesService, ElectricitySalesService>();
builder.Services.AddScoped<IHttpRequestService, HttpRequestService>();
builder.Services.AddScoped<IJobService, SalesJobService>();
builder.Services.AddScoped<RetrieveJobs>();
builder.Services.AddScoped<RequestExecutionContext>();

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
app.UseHangfireDashboard();

// Map Controllers
app.MapControllers();

app.Run();
