using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (optional)
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Optional: Swagger UI
if (app.Environment.IsDevelopment()) {
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

// Define a simple endpoint
app.MapGet("/", () => "Welcome to the Chat Messaging Service!");
app.MapPost("/send", (string message) => {
    return Results.Ok($"Nachricht empfangen: {message}");
});
app.MapPost("/login", () => {
    return Results.Ok("Login erfolgreich!");
});

// Run the web server
app.Run("http://localhost:5000");