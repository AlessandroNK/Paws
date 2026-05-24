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

// Add services
builder.Services.AddControllers();

var app = builder.Build();

// Map controllers
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();