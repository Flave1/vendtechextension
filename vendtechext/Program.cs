using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Configurations;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Middleware;
using vendtechext.BLL.Services;
using vendtechext.DAL.Models;
using signalrserver.HubConnection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:56549", "https://vendtechsl.com")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials()
                   .SetIsOriginAllowed((host) => true);
        });
});

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<RTSInformation>(builder.Configuration.GetSection("RTSInformation"));
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
});
builder.Services.AddSignalR();

builder.Services.AddScoped<IB2bAccountService, B2bAccountService>();
builder.Services.AddTransient<IErrorlogService, ErrorlogService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowSpecificOrigin");

app.MapHub<CustomersHub>("/customerHub");
app.MapHub<AdminHub>("/adminHub");

app.MapControllers();
app.Run();