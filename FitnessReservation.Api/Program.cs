using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger / OpenAPI (dev version)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddSingleton<BasePriceProvider>();
builder.Services.AddSingleton<MultiplierProvider>();
builder.Services.AddSingleton<PricingEngine>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Minimal health endpoint (useful for Docker/CI later)
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Pricing endpoint (API v1: membership comes from request body)
app.MapPost("/pricing/calculate", (PricingRequest request, PricingEngine engine) =>
{
    var result = engine.Calculate(request);
    return Results.Ok(result);
})
.WithName("CalculatePrice");
app.Run();
