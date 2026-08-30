using OficinaApi.Presentation.Configuration;
using OficinaApi.Presentation.ExceptionFilters;
using OficinaApi.Presentation.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOTelConfiguration(builder.Configuration);
builder.Logging.AddOTelLogging();
builder.Services.AddAutenticationConfiguration(builder.Configuration);
builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHealthChecks();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwaggerConfiguration();
app.UseAutenticationConfiguration();

app.UseMiddleware<UserSessionEnrichmentMiddleware>();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

await app.UseAdminUserConfiguration();

app.Run();
