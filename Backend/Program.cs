using Backend.Core.Controllers.interfaces;
using Backend.Core.Data;
using Backend.Core.Repositories;
using Backend.Core.Repositories.Interfaces;
using Backend.Core.Services;
using Backend.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// Add services to the container.
builder.Services.AddControllers();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IAppointmentsRepository, AppointmentsRepository>();
builder.Services.AddScoped<IAppConfigRepository, AppConfigRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAppointmentsService, AppointmentsService>();
builder.Services.AddScoped<IAppConfigService, AppConfigService>();

// Endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Swagger
builder.Services.AddSwaggerGen();

// Environment
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// DB
var connectionString = environment switch
{
    "Development" => Environment.GetEnvironmentVariable("SUPABASE_PAWS_DEV"),
    _ => Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_SUPABASE_PROD")
};
connectionString +=
    ";Timeout=30;CommandTimeout=60;Read Buffer Size=8192;Pooling=true;Connection Idle Lifetime=10;KeepAlive=30;No Reset On Close=true;Enlist=false;Tcp Keepalive=true;Max Auto Prepare=0;";

if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("No valid connection string found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.CommandTimeout(20)),
    ServiceLifetime.Scoped
);

// Build the app
var app = builder.Build();

// Run internal services inits and tasks before starting the app
using (var scope = app.Services.CreateScope())
{
    // Init App config
    var appConfigureService = scope.ServiceProvider.GetRequiredService<IAppConfigService>();
    var result = await appConfigureService.InitializeAsync();
    if (!result) throw new InvalidOperationException("Failed to initialize app config");

    // Init Appointments service
    var appointmentsService = scope.ServiceProvider.GetRequiredService<IAppointmentsService>();
    result = await appointmentsService.PopulateAppointments();
    if (!result) throw new InvalidOperationException("Failed to populate appointments");
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Map controllers
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();