using FitnessReservation.Api.Auth;
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

builder.Services.AddSingleton<IPeakHourPolicy, PeakHourPolicy>();
builder.Services.AddSingleton<IOccupancyClassifier, OccupancyClassifier>();

builder.Services.AddSingleton<ReservationsService>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "FR_SID";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // local http
});

builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IMemberRepository, InMemoryMemberRepository>();

builder.Services.AddSingleton<IMembershipCodeRepository>(_ =>
    new InMemoryMembershipCodeRepository(new[]
    {
        new MembershipCode { Code = "STU-2025", MembershipType = MembershipType.Student, IsActive = true },
        new MembershipCode { Code = "PRM-2025", MembershipType = MembershipType.Premium, IsActive = true },
        new MembershipCode { Code = "STD-OPEN", MembershipType = MembershipType.Standard, IsActive = true },
    }));

var app = builder.Build();

app.UseSession();

static bool TryGetUser(HttpContext ctx, IMemberRepository members, out Member member)
{
    member = default!;
    if (!AuthSession.TryGetMemberId(ctx, out var memberId))
        return false;

    var m = members.Get(memberId);
    if (m is null)
        return false;

    member = m;
    return true;
}

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

app.MapPost("/auth/register", (
    RegisterRequest body,
    IMemberRepository members,
    IMembershipCodeRepository codes,
    IPasswordService passwords) =>
{
    if (string.IsNullOrWhiteSpace(body.Username) || string.IsNullOrWhiteSpace(body.Password))
        return Results.BadRequest(new { error = "InvalidInput" });

    if (body.MembershipType is MembershipType.Student or MembershipType.Premium)
    {
        if (string.IsNullOrWhiteSpace(body.MembershipCode))
            return Results.BadRequest(new { error = "MissingMembershipCode" });

        var code = codes.Get(body.MembershipCode);
        if (code is null || !code.IsActive || code.UsedByMemberId is not null || code.MembershipType != body.MembershipType)
            return Results.BadRequest(new { error = "InvalidMembershipCode" });
    }

    var member = new Member
    {
        MemberId = Guid.NewGuid(),
        Username = body.Username,
        PasswordHash = passwords.Hash(body.Password),
        MembershipType = body.MembershipType
    };

    try
    {
        members.Add(member);
    }
    catch (InvalidOperationException ex) when (ex.Message == "UsernameTaken")
    {
        return Results.Conflict(new { error = "UsernameTaken" });
    }

    if (body.MembershipType is MembershipType.Student or MembershipType.Premium)
        codes.MarkUsed(body.MembershipCode!, member.MemberId);

    return Results.Created($"/members/{member.MemberId}",
        new RegisterResponse(member.MemberId, member.Username, member.MembershipType));
})
.WithName("Register");

app.MapPost("/auth/login", (
    HttpContext ctx,
    LoginRequest body,
    IMemberRepository members,
    IPasswordService passwords) =>
{
    var member = members.FindByUsername(body.Username);
    if (member is null)
        return Results.Unauthorized();

    if (!passwords.Verify(member.PasswordHash, body.Password))
        return Results.Unauthorized();

    AuthSession.SignIn(ctx, member.MemberId);

    return Results.Ok(new LoginResponse(member.MemberId, member.Username, member.MembershipType));
})
.WithName("Login");

app.MapPost("/auth/logout", (HttpContext ctx) =>
{
    AuthSession.SignOut(ctx);
    return Results.Ok(new { status = "logged-out" });
})
.WithName("Logout");

app.MapGet("/me", (HttpContext ctx, IMemberRepository members) =>
{
    if (!TryGetUser(ctx, members, out var me))
        return Results.Unauthorized();

    return Results.Ok(new MeResponse(me.MemberId, me.Username, me.MembershipType));
})
.WithName("Me");

app.MapPost("/pricing/calculate", (PricingRequest request, PricingEngine engine) =>
{
    var result = engine.Calculate(request);
    return Results.Ok(result);
})
.WithName("CalculatePrice");

app.MapGet("/sessions", (
    HttpContext ctx,
    SportType sport,
    DateTime from,
    DateTime to,
    IMemberRepository members,
    ISessionRepository sessions,
    IReservationRepository reservations,
    PricingEngine pricing,
    IPeakHourPolicy peakPolicy,
    IOccupancyClassifier occupancyClassifier) =>
{
    if (!TryGetUser(ctx, members, out var me))
        return Results.Unauthorized();

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
                Membership = me.MembershipType,
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
    HttpContext ctx,
    CreateReservationRequest body,
    IMemberRepository members,
    ReservationsService service) =>
{
    if (!TryGetUser(ctx, members, out var me))
        return Results.Unauthorized();

    var result = service.Reserve(new ReserveRequest
    {
        MemberId = me.MemberId.ToString(),
        SessionId = body.SessionId,
        Membership = me.MembershipType
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

public sealed record CreateReservationRequest(Guid SessionId);

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

public sealed record RegisterRequest(
    string Username,
    string Password,
    MembershipType MembershipType,
    string? MembershipCode);

public sealed record RegisterResponse(
    Guid MemberId,
    string Username,
    MembershipType MembershipType);

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(
    Guid MemberId,
    string Username,
    MembershipType MembershipType);

public sealed record MeResponse(
    Guid MemberId,
    string Username,
    MembershipType MembershipType);
