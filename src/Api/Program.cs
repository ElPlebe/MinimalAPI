using Api.Features.Examples;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddProblemDetails();     // Consistent RFC 7807 error payloads
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Example API", Version = "v1" });
});

builder.Services.AddSingleton<IExampleRepository, InMemoryExampleRepository>();

var app = builder.Build();

// Middleware
app.UseExceptionHandler();    // Uses ProblemDetails by default when registered
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();

// API v1 group
var v1 = app.MapGroup("/api/v1").WithOpenApi();

v1.MapExampleEndpoints();

app.Run();

// Needed for WebApplicationFactory<Program> in tests
public partial class Program { }

//Change only for create a PR....
