using MessagePack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
app.MapGet("/", () => "Type=ChatMessagingService");

// Run the web server
app.Run("http://localhost:5010/");