using OficinaApi.WebApi.Configuration;
using OficinaApi.WebApi.ExceptionFilters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutenticationConfiguration(builder.Configuration);
builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

var app = builder.Build();

app.UseSwaggerConfiguration();
app.UseAutenticationConfiguration();

app.MapControllers();

await app.UseAdminUserConfiguration();

app.Run();
