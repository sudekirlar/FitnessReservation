using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Reservations.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// JSON options
// --------------------
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// --------------------
// Swagger / OpenAPI
// --------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --------------------
// Pricing DI
// --------------------
builder.Services.AddSingleton<BasePriceProvider>();
builder.Services.AddSingleton<MultiplierProvider>();
builder.Services.AddSingleton<PricingEngine>();

// --------------------
// Reservations DI
// --------------------
builder.Services.AddSingleton<InMemorySessionRepository>();
builder.Services.AddSingleton<ISessionRepository>(sp =>
    sp.GetRequiredService<InMemorySessionRepository>());

builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<ReservationsService>();

var app = builder.Build();

// --------------------
// Swagger middleware
// --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --------------------
// Seed sessions (DB yokken deterministic davranýþ için)
// --------------------
var sessionRepo = app.Services.GetRequiredService<InMemorySessionRepository>();

var futureSessionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var pastSessionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

sessionRepo.Upsert(new ClassSession
{
    SessionId = futureSessionId,
    Sport = SportType.Yoga,
    StartsAtUtc = DateTime.UtcNow.AddHours(2),
    Capacity = 1
});

sessionRepo.Upsert(new ClassSession
{
    SessionId = pastSessionId,
    Sport = SportType.Yoga,
    StartsAtUtc = DateTime.UtcNow.AddHours(-2),
    Capacity = 10
});

// --------------------
// Health
// --------------------
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// --------------------
// Pricing endpoint
// --------------------
app.MapPost("/pricing/calculate", (PricingRequest request, PricingEngine engine) =>
{
    var result = engine.Calculate(request);
    return Results.Ok(result);
})
.WithName("CalculatePrice");

// --------------------
// Reservations endpoint
// --------------------
app.MapPost("/reservations", (
    CreateReservationRequest body,
    ReservationsService service) =>
{
    var result = service.Reserve(new ReserveRequest
    {
        MemberId = body.MemberId,
        SessionId = body.SessionId,
        Membership = body.Membership
    });

    if (result.Success)
    {
        return Results.Created(
            $"/reservations/{result.ReservationId}",
            new CreateReservationResponse(
                result.ReservationId!.Value,
                result.PriceSnapshot!.FinalPrice));
    }

    return result.Error switch
    {
        ReserveError.SessionNotFound => Results.NotFound(new { error = "SessionNotFound" }),
        ReserveError.SessionInPast => Results.BadRequest(new { error = "SessionInPast" }),
        ReserveError.DuplicateReservation => Results.Conflict(new { error = "DuplicateReservation" }),
        ReserveError.CapacityFull => Results.Conflict(new { error = "CapacityFull" }),
        _ => Results.Problem("Unknown reservation error")
    };
})
.WithName("CreateReservation");

app.Run();

// --------------------
// DTOs (API contract)
// --------------------
public sealed record CreateReservationRequest(
    string MemberId,
    Guid SessionId,
    MembershipType Membership);

public sealed record CreateReservationResponse(
    Guid ReservationId,
    decimal FinalPrice);
