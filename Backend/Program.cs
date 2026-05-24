using Backend.Core.Services;
using Backend.Core.Services.Interfaces;

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

// Services
builder.Services.AddScoped<IUserService, UserService>();

// Endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Swagger
builder.Services.AddSwaggerGen();

// Repositories
var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Map controllers
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();