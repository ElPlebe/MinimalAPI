using Api.Features.Examples;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddProblemDetails();              // RFC 7807
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Example API", Version = "v1" });
});

builder.Services.AddSingleton<IExampleRepository, InMemoryExampleRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseExceptionHandler(); // integrate ProblemDetails with the standard middleware

// API v1 group
var v1 = app.MapGroup("/api/v1").WithOpenApi();

v1.MapExampleEndpoints();

app.Run();

// Required for WebApplicationFactory<Program> in tests
public partial class Program { }
