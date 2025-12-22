using FitnessReservation.Api.Auth;
using FitnessReservation.Auth;
using FitnessReservation.Persistence;
using FitnessReservation.Persistence.Entities;
using FitnessReservation.Persistence.Repos;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using FitnessReservation.Reservations.Models;
using FitnessReservation.Reservations.Repos;
using FitnessReservation.Reservations.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- CORS AYARI ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174") 
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); 
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDbContext<FitnessReservationDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<BasePriceProvider>();
builder.Services.AddSingleton<MultiplierProvider>();
builder.Services.AddSingleton<PricingEngine>();

builder.Services.AddScoped<ISessionRepository, EfSessionRepository>();
builder.Services.AddScoped<IReservationRepository, EfReservationRepository>();

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IPeakHourPolicy, PeakHourPolicy>();
builder.Services.AddSingleton<IOccupancyClassifier, OccupancyClassifier>();

builder.Services.AddScoped<ReservationsService>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "FR_SID";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IMemberRepository, EfMemberRepository>();
builder.Services.AddScoped<IMembershipCodeRepository, EfMembershipCodeRepository>();

builder.Services.AddCookiePolicy(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.None;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
});

var app = builder.Build();

app.UseCors("AllowReactApp");

if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<FitnessReservationDbContext>();

        var connStr = db.Database.GetDbConnection().ConnectionString ?? string.Empty;
        var isInMemorySqlite = db.Database.IsSqlite() &&
                               connStr.Contains(":memory:", StringComparison.OrdinalIgnoreCase);

        if (isInMemorySqlite)
            db.Database.EnsureCreated();
        else
            db.Database.Migrate();

        using var tx = db.Database.BeginTransaction();

        // --- ÜYELİK KODLARI (SEED) ---
        SeedMembershipCode(db, "STU-2025", MembershipType.Student, true);
        SeedMembershipCode(db, "PRM-2025", MembershipType.Premium, true);
        SeedMembershipCode(db, "STD-OPEN", MembershipType.Standard, true);

        // Ekstra Kodlar
        SeedMembershipCode(db, "STU-2025-01", MembershipType.Student, true);
        SeedMembershipCode(db, "STU-2025-02", MembershipType.Student, true);
        SeedMembershipCode(db, "STU-2025-03", MembershipType.Student, true);
        
        SeedMembershipCode(db, "PRM-2025-01", MembershipType.Premium, true);
        SeedMembershipCode(db, "PRM-2025-02", MembershipType.Premium, true);
        SeedMembershipCode(db, "PRM-2025-03", MembershipType.Premium, true);

        // VIP Kod
        SeedMembershipCode(db, "UTK-VIP", MembershipType.Premium, true);

        // --- SEANSLAR (SEED) ---
        var y = DateTime.UtcNow.Year;
        var m = DateTime.UtcNow.Month;

        // PILATES (22-28)
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0022"), SportType.Pilates, new DateTime(y, m, 22, 7, 30, 0, DateTimeKind.Utc), 16, "Derya Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0122"), SportType.Pilates, new DateTime(y, m, 22, 12, 0, 0, DateTimeKind.Utc), 12, "Selin Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0222"), SportType.Pilates, new DateTime(y, m, 22, 19, 0, 0, DateTimeKind.Utc), 18, "Merve Hoca");
        // ... Diğer günler ve sporlar (Mevcut kodlarınızdaki gibi kalabilir, burayı kısaltıyorum) ...
        
        // ZUMBA (Örnek)
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0022"), SportType.Zumba, new DateTime(y, m, 22, 12, 0, 0, DateTimeKind.Utc), 22, "Zeynep Hoca");

        db.SaveChanges();
        tx.Commit();

        // --- DEBUG: KODLARI KONSOLA YAZDIR ---
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- VERİTABANINDAKİ KUPON KODLARI ---");
        foreach(var c in db.MembershipCodes.ToList())
        {
            var status = c.UsedByMemberId == null ? "MÜSAİT (Kullanılabilir)" : "KULLANILMIŞ (Hata Verir)";
            Console.WriteLine($"KOD: {c.Code.PadRight(15)} | TİP: {c.MembershipType.ToString().PadRight(10)} | DURUM: {status}");
        }
        Console.WriteLine("-------------------------------------\n");
        Console.ResetColor();
    }
}

app.UseCookiePolicy();
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

    app.MapPost("/__test/reset", (FitnessReservationDbContext db) =>
    {
        db.Reservations.RemoveRange(db.Reservations);
        db.Members.RemoveRange(db.Members);
        db.MembershipCodes.RemoveRange(db.MembershipCodes);
        db.SaveChanges();
        return Results.Ok(new { status = "reset-ok" });
    });
}

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
    // Misafir Modu
    MembershipType userMembership = MembershipType.Standard;
    if (TryGetUser(ctx, members, out var me))
    {
        userMembership = me.MembershipType;
    }

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
                Membership = userMembership,
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

static void SeedMembershipCode(FitnessReservationDbContext db, string code, MembershipType type, bool isActive)
{
    var existing = db.MembershipCodes.SingleOrDefault(x => x.Code == code);
    if (existing is null)
    {
        db.MembershipCodes.Add(new MembershipCodeEntity
        {
            Code = code,
            MembershipType = type,
            IsActive = isActive,
            UsedByMemberId = null
        });
    }
    else
    {
        existing.MembershipType = type;
        existing.IsActive = isActive;
        // KODLARI SIFIRLA (Kullanılmış olsa bile boşa çıkar)
        existing.UsedByMemberId = null;
    }
}

static void SeedSession(
    FitnessReservationDbContext db,
    Guid sessionId,
    SportType sport,
    DateTime startsAtUtc,
    int capacity,
    string instructorName)
{
    var existing = db.Sessions.SingleOrDefault(x => x.SessionId == sessionId);
    if (existing is null)
    {
        db.Sessions.Add(new SessionEntity
        {
            SessionId = sessionId,
            Sport = sport,
            StartsAtUtc = startsAtUtc,
            Capacity = capacity,
            InstructorName = instructorName
        });
    }
    else
    {
        existing.Sport = sport;
        existing.StartsAtUtc = startsAtUtc;
        existing.Capacity = capacity;
        existing.InstructorName = instructorName;
    }
}

public sealed record CreateReservationRequest(Guid SessionId);

public sealed record CreateReservationResponse(Guid ReservationId, decimal FinalPrice);

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

public sealed record RegisterRequest(string Username, string Password, MembershipType MembershipType, string? MembershipCode);

public sealed record RegisterResponse(Guid MemberId, string Username, MembershipType MembershipType);

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(Guid MemberId, string Username, MembershipType MembershipType);

public sealed record MeResponse(Guid MemberId, string Username, MembershipType MembershipType);