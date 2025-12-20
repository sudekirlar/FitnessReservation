using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Reservations.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<BasePriceProvider>();
builder.Services.AddSingleton<MultiplierProvider>();
builder.Services.AddSingleton<PricingEngine>();

builder.Services.AddSingleton<InMemorySessionRepository>();
builder.Services.AddSingleton<ISessionRepository>(sp =>
    sp.GetRequiredService<InMemorySessionRepository>());

builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<ReservationsService>();

builder.Services.AddSingleton<IPeakHourPolicy, PeakHourPolicy>();
builder.Services.AddSingleton<IOccupancyClassifier, OccupancyClassifier>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapPost("/__test/reset", (IReservationRepository reservations) =>
    {
        if (reservations is InMemoryReservationRepository mem)
        {
            mem.Clear();
            return Results.Ok(new { status = "reset-ok" });
        }

        return Results.Problem("Reset is only supported for InMemoryReservationRepository.");
    });
}

var sessionRepo = app.Services.GetRequiredService<InMemorySessionRepository>();

var futureGeneralSessionId =
    Guid.Parse("11111111-1111-1111-1111-111111111111");

sessionRepo.Upsert(new ClassSession
{
    SessionId = futureGeneralSessionId,
    Sport = SportType.Yoga,
    StartsAtUtc = DateTime.UtcNow.AddHours(2),
    Capacity = 100,
    InstructorName = "Elif Hoca"
});

var futureCapacity1SessionId =
    Guid.Parse("33333333-3333-3333-3333-333333333333");

sessionRepo.Upsert(new ClassSession
{
    SessionId = futureCapacity1SessionId,
    Sport = SportType.Yoga,
    StartsAtUtc = DateTime.UtcNow.AddHours(2),
    Capacity = 1,
    InstructorName = "Hasan Hoca"
});

var pastSessionId =
    Guid.Parse("22222222-2222-2222-2222-222222222222");

sessionRepo.Upsert(new ClassSession
{
    SessionId = pastSessionId,
    Sport = SportType.Yoga,
    StartsAtUtc = DateTime.UtcNow.AddHours(-2),
    Capacity = 10,
    InstructorName = "Sibel Hoca"
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/pricing/calculate", (PricingRequest request, PricingEngine engine) =>
{
    var result = engine.Calculate(request);
    return Results.Ok(result);
})
.WithName("CalculatePrice");

app.MapGet("/sessions", (
    SportType sport,
    DateTime from,
    DateTime to,
    MembershipType membership,
    ISessionRepository sessions,
    IReservationRepository reservations,
    PricingEngine pricing,
    IPeakHourPolicy peakPolicy,
    IOccupancyClassifier occupancyClassifier) =>
{
    if (from >= to)
        return Results.BadRequest(new { error = "InvalidDateRange" });

    var items = sessions.Query(sport, from, to)
        .Select(s =>
        {
            var reservedCount = reservations.CountBySession(s.SessionId);

            var isPeak = peakPolicy.IsPeak(s.StartsAtUtc);
            var occupancy = occupancyClassifier.Classify(reservedCount, s.Capacity);

            var price = pricing.Calculate(new PricingRequest
            {
                Sport = s.Sport,
                Membership = membership,
                IsPeak = isPeak,
                Occupancy = occupancy
            });

            return new SessionListItem(
                s.SessionId,
                s.Sport,
                s.StartsAtUtc,
                s.InstructorName,
                s.Capacity,
                reservedCount,
                isPeak,
                occupancy,
                price);
        })
        .ToList();

    return Results.Ok(items);
})
.WithName("ListSessions");


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

public sealed record CreateReservationRequest(
    string MemberId,
    Guid SessionId,
    MembershipType Membership);

public sealed record CreateReservationResponse(
    Guid ReservationId,
    decimal FinalPrice);

public sealed record SessionListItem(
    Guid SessionId,
    SportType Sport,
    DateTime StartsAtUtc,
    string InstructorName,
    int Capacity,
    int ReservedCount,
    bool IsPeak,
    OccupancyLevel OccupancyLevel,
    FitnessReservation.Pricing.Models.PricingResult Price);
