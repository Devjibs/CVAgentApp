using CVAgentApp.API.Swagger;
using CVAgentApp.Core.Interfaces;
using CVAgentApp.Infrastructure.Agents;
using CVAgentApp.Infrastructure.Data;
using CVAgentApp.Infrastructure.Guardrails;
using CVAgentApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Temporarily disable Swagger to test API functionality
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo
//     {
//         Title = "CV Agent API",
//         Version = "v1",
//         Description = "AI-powered CV generation and job matching API"
//     });
//     
//     // Configure file upload support
//     c.OperationFilter<FileUploadOperationFilter>();
//     
//     // Add security definition for API key
//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
//         Name = "Authorization",
//         In = ParameterLocation.Header,
//         Type = SecuritySchemeType.ApiKey,
//         Scheme = "Bearer"
//     });
// });

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HTTP client for external requests
builder.Services.AddHttpClient();

// Add custom services
builder.Services.AddScoped<ICVGenerationService, CVGenerationService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IEvaluationService, EvaluationService>();
builder.Services.AddScoped<IMonitoringService, MonitoringService>();

// Add agent services
builder.Services.AddScoped<ICVParsingAgent, CVParsingAgent>();
builder.Services.AddScoped<IJobExtractionAgent, JobExtractionAgent>();
builder.Services.AddScoped<IMatchingAgent, MatchingAgent>();
builder.Services.AddScoped<ICVGenerationAgent, CVGenerationAgent>();
builder.Services.AddScoped<IReviewAgent, ReviewAgent>();
builder.Services.AddScoped<IMultiAgentOrchestrator, MultiAgentOrchestrator>();

// Add guardrail services
builder.Services.AddScoped<IGuardrailService, GuardrailService>();
builder.Services.AddScoped<IInputGuardrail, JobPostingGuardrail>();
builder.Services.AddScoped<IInputGuardrail, CVContentGuardrail>();
builder.Services.AddScoped<IOutputGuardrail, TruthfulnessGuardrail>();
builder.Services.AddScoped<IOutputGuardrail, DocumentQualityGuardrail>();
builder.Services.AddScoped<IOutputGuardrail, ComplianceGuardrail>();

// Register guardrails with the service
builder.Services.AddHostedService<GuardrailRegistrationService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Temporarily disable Swagger to test API functionality
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Keep the weather forecast endpoint for testing
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
