using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Middleware;
using vendtechext.BLL.Services;
using vendtechext.DAL.Models;
using signalrserver.HubConnection;
using Hangfire;
using vendtechext.Helper;
using vendtechext.Helper.Configurations;
using vendtechext.BLL.Repository;
using vendtechext.BLL.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using vendtechext.BLL.Common;
using vendtechext.BLL.Services.RecurringJobs;

var builder = WebApplication.CreateBuilder(args);

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
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


builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Identity configuration
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders()
.AddTokenProvider<CustomTokenProvider<AppUser>>("vendtech");

// Configure JWT Authentication


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "vendtech",
        ValidAudience = "vendtech", // Ensure this matches the audience claim in the token
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
    };
});
// Swagger Configuration

builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VENDTECHSL API",
        Version = "v1"
    });

    // Add JWT Bearer Authorization to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please insert JWT token into field"
    });

    // Apply JWT to all operations
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

// SignalR Configuration
builder.Services.AddSignalR();

//Cache
builder.Services.AddMemoryCache();

// Dependency Injection
builder.Services.AddScoped<IVendtechReconcillationService, VendtechReconcillationService>();
builder.Services.AddScoped<IIntegratorService, IntegratorService>();
builder.Services.AddScoped<IMobilePushService, MobilePushService>();
builder.Services.AddScoped<IAPISalesService, APISalesService>();
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddScoped<VendtechTransactionsService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<TransactionIdGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<RequestExecutionContext>();
builder.Services.AddScoped<TransactionRepository>();
builder.Services.AddScoped<NotificationHelper>();
builder.Services.AddScoped<HttpRequestService>();
builder.Services.AddScoped<WalletRepository>();
builder.Services.AddScoped<AppConfiguration>();
builder.Services.AddScoped<EmailHelper>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<FileHelper>();

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
app.UseAuthentication(); 
app.UseAuthorization();

//Seed Default User
///
///REMOVE THE COMMENT TO SEED INTO A NEW DATABASE
///
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    await SeedData.Initialize(services);
//    await SeedData.Settings(services);
//    await SeedData.PaymentMethods(services);
//}

// Map Controllers
app.MapControllers();

//Recurring Jobs
var integratorBalanceJob = new IntegratorBalanceJob();
RecurringJob.AddOrUpdate("midnight-job", () => integratorBalanceJob.Run(), "0 0 * * *");


FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fb_private_key.json")),
});
app.UseStaticFiles();
app.Run();
