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

        SeedMembershipCode(db, "STU-2025", MembershipType.Student, true);
        SeedMembershipCode(db, "PRM-2025", MembershipType.Premium, true);
        SeedMembershipCode(db, "STD-OPEN", MembershipType.Standard, true);

        SeedMembershipCode(db, "STU-2025-01", MembershipType.Student, true);
        SeedMembershipCode(db, "STU-2025-02", MembershipType.Student, true);
        SeedMembershipCode(db, "STU-2025-03", MembershipType.Student, true);
        SeedMembershipCode(db, "PRM-2025-01", MembershipType.Premium, true);
        SeedMembershipCode(db, "PRM-2025-02", MembershipType.Premium, true);
        SeedMembershipCode(db, "PRM-2025-03", MembershipType.Premium, true);

        SeedMembershipCode(db, "UTK-VIP", MembershipType.Premium, true);

        // ------------------------------------------------------------
        // Seed: Pilates (22 -> 28) daily (2-3 sessions/day), realistic hours/capacities
        // ------------------------------------------------------------
        var y = DateTime.UtcNow.Year;
        var m = DateTime.UtcNow.Month;

        // 22
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0022"), SportType.Pilates, new DateTime(y, m, 22, 7, 30, 0, DateTimeKind.Utc), 16, "Derya Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0122"), SportType.Pilates, new DateTime(y, m, 22, 12, 0, 0, DateTimeKind.Utc), 12, "Selin Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0222"), SportType.Pilates, new DateTime(y, m, 22, 19, 0, 0, DateTimeKind.Utc), 18, "Merve Hoca");

        // 23
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0023"), SportType.Pilates, new DateTime(y, m, 23, 8, 0, 0, DateTimeKind.Utc), 14, "Ece Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0123"), SportType.Pilates, new DateTime(y, m, 23, 17, 0, 0, DateTimeKind.Utc), 15, "Gizem Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0223"), SportType.Pilates, new DateTime(y, m, 23, 20, 30, 0, DateTimeKind.Utc), 20, "Derya Hoca");

        // 24
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0024"), SportType.Pilates, new DateTime(y, m, 24, 9, 0, 0, DateTimeKind.Utc), 10, "Selin Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0124"), SportType.Pilates, new DateTime(y, m, 24, 13, 0, 0, DateTimeKind.Utc), 12, "Merve Hoca");

        // 25
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0025"), SportType.Pilates, new DateTime(y, m, 25, 7, 0, 0, DateTimeKind.Utc), 18, "Ece Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0125"), SportType.Pilates, new DateTime(y, m, 25, 18, 30, 0, DateTimeKind.Utc), 16, "Gizem Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0225"), SportType.Pilates, new DateTime(y, m, 25, 21, 0, 0, DateTimeKind.Utc), 22, "Selin Hoca");

        // 26
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0026"), SportType.Pilates, new DateTime(y, m, 26, 10, 0, 0, DateTimeKind.Utc), 14, "Merve Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0126"), SportType.Pilates, new DateTime(y, m, 26, 19, 30, 0, DateTimeKind.Utc), 18, "Derya Hoca");

        // 27
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0027"), SportType.Pilates, new DateTime(y, m, 27, 8, 30, 0, DateTimeKind.Utc), 12, "Selin Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0127"), SportType.Pilates, new DateTime(y, m, 27, 16, 0, 0, DateTimeKind.Utc), 15, "Ece Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0227"), SportType.Pilates, new DateTime(y, m, 27, 20, 0, 0, DateTimeKind.Utc), 20, "Gizem Hoca");

        // 28
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0028"), SportType.Pilates, new DateTime(y, m, 28, 9, 30, 0, DateTimeKind.Utc), 14, "Derya Hoca");
        SeedSession(db, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0128"), SportType.Pilates, new DateTime(y, m, 28, 18, 0, 0, DateTimeKind.Utc), 16, "Merve Hoca");

        // ------------------------------------------------------------
        // Seed: Other sports (Yoga, Spinning, HIIT, Zumba) ALSO daily 22 -> 28
        // (2-3 sessions/day; some peak hours 18-22)
        // ------------------------------------------------------------

        // YOGA (daily)
        // 22
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0022"), SportType.Yoga, new DateTime(y, m, 22, 9, 0, 0, DateTimeKind.Utc), 20, "Ayşe Hoca");
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0122"), SportType.Yoga, new DateTime(y, m, 22, 19, 30, 0, DateTimeKind.Utc), 24, "Elif Hoca");
        // 23
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0023"), SportType.Yoga, new DateTime(y, m, 23, 8, 30, 0, DateTimeKind.Utc), 18, "Sibel Hoca");
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0123"), SportType.Yoga, new DateTime(y, m, 23, 18, 0, 0, DateTimeKind.Utc), 22, "Ayşe Hoca");
        // 24
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0024"), SportType.Yoga, new DateTime(y, m, 24, 12, 0, 0, DateTimeKind.Utc), 20, "Elif Hoca");
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0124"), SportType.Yoga, new DateTime(y, m, 24, 20, 0, 0, DateTimeKind.Utc), 24, "Sibel Hoca");
        // 25
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0025"), SportType.Yoga, new DateTime(y, m, 25, 9, 30, 0, DateTimeKind.Utc), 18, "Ayşe Hoca");
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0125"), SportType.Yoga, new DateTime(y, m, 25, 21, 0, 0, DateTimeKind.Utc), 22, "Elif Hoca");
        // 26
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0026"), SportType.Yoga, new DateTime(y, m, 26, 8, 0, 0, DateTimeKind.Utc), 20, "Sibel Hoca");
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0126"), SportType.Yoga, new DateTime(y, m, 26, 18, 30, 0, DateTimeKind.Utc), 24, "Ayşe Hoca");
        // 27
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0027"), SportType.Yoga, new DateTime(y, m, 27, 10, 0, 0, DateTimeKind.Utc), 18, "Elif Hoca");
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0127"), SportType.Yoga, new DateTime(y, m, 27, 20, 30, 0, DateTimeKind.Utc), 22, "Sibel Hoca");
        // 28
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0028"), SportType.Yoga, new DateTime(y, m, 28, 9, 0, 0, DateTimeKind.Utc), 20, "Ayşe Hoca");
        SeedSession(db, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0128"), SportType.Yoga, new DateTime(y, m, 28, 19, 0, 0, DateTimeKind.Utc), 24, "Elif Hoca");

        // SPINNING (daily)
        // 22
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0022"), SportType.Spinning, new DateTime(y, m, 22, 7, 0, 0, DateTimeKind.Utc), 16, "Can Hoca");
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0122"), SportType.Spinning, new DateTime(y, m, 22, 18, 30, 0, DateTimeKind.Utc), 20, "Berk Hoca");
        // 23
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0023"), SportType.Spinning, new DateTime(y, m, 23, 12, 0, 0, DateTimeKind.Utc), 18, "Can Hoca");
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0123"), SportType.Spinning, new DateTime(y, m, 23, 20, 0, 0, DateTimeKind.Utc), 22, "Berk Hoca");
        // 24
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0024"), SportType.Spinning, new DateTime(y, m, 24, 9, 0, 0, DateTimeKind.Utc), 16, "Berk Hoca");
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0124"), SportType.Spinning, new DateTime(y, m, 24, 19, 30, 0, DateTimeKind.Utc), 20, "Can Hoca");
        // 25
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0025"), SportType.Spinning, new DateTime(y, m, 25, 8, 30, 0, DateTimeKind.Utc), 18, "Can Hoca");
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0125"), SportType.Spinning, new DateTime(y, m, 25, 19, 0, 0, DateTimeKind.Utc), 22, "Berk Hoca");
        // 26
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0026"), SportType.Spinning, new DateTime(y, m, 26, 12, 30, 0, DateTimeKind.Utc), 16, "Can Hoca");
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0126"), SportType.Spinning, new DateTime(y, m, 26, 21, 0, 0, DateTimeKind.Utc), 20, "Berk Hoca");
        // 27
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0027"), SportType.Spinning, new DateTime(y, m, 27, 9, 0, 0, DateTimeKind.Utc), 18, "Berk Hoca");
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0127"), SportType.Spinning, new DateTime(y, m, 27, 18, 0, 0, DateTimeKind.Utc), 22, "Can Hoca");
        // 28
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0028"), SportType.Spinning, new DateTime(y, m, 28, 8, 0, 0, DateTimeKind.Utc), 16, "Can Hoca");
        SeedSession(db, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccc0128"), SportType.Spinning, new DateTime(y, m, 28, 20, 0, 0, DateTimeKind.Utc), 20, "Berk Hoca");

        // HIIT (daily)
        // 22
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0022"), SportType.HIIT, new DateTime(y, m, 22, 8, 0, 0, DateTimeKind.Utc), 12, "Deniz Hoca");
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0122"), SportType.HIIT, new DateTime(y, m, 22, 19, 0, 0, DateTimeKind.Utc), 14, "Eren Hoca");
        // 23
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0023"), SportType.HIIT, new DateTime(y, m, 23, 7, 0, 0, DateTimeKind.Utc), 14, "Deniz Hoca");
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0123"), SportType.HIIT, new DateTime(y, m, 23, 20, 30, 0, DateTimeKind.Utc), 12, "Eren Hoca");
        // 24
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0024"), SportType.HIIT, new DateTime(y, m, 24, 12, 0, 0, DateTimeKind.Utc), 16, "Eren Hoca");
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0124"), SportType.HIIT, new DateTime(y, m, 24, 18, 30, 0, DateTimeKind.Utc), 14, "Deniz Hoca");
        // 25
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0025"), SportType.HIIT, new DateTime(y, m, 25, 9, 0, 0, DateTimeKind.Utc), 16, "Deniz Hoca");
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0125"), SportType.HIIT, new DateTime(y, m, 25, 21, 0, 0, DateTimeKind.Utc), 12, "Eren Hoca");
        // 26
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0026"), SportType.HIIT, new DateTime(y, m, 26, 10, 0, 0, DateTimeKind.Utc), 14, "Eren Hoca");
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0126"), SportType.HIIT, new DateTime(y, m, 26, 18, 30, 0, DateTimeKind.Utc), 16, "Deniz Hoca");
        // 27
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0027"), SportType.HIIT, new DateTime(y, m, 27, 12, 0, 0, DateTimeKind.Utc), 14, "Deniz Hoca");
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0127"), SportType.HIIT, new DateTime(y, m, 27, 21, 0, 0, DateTimeKind.Utc), 12, "Eren Hoca");
        // 28
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0028"), SportType.HIIT, new DateTime(y, m, 28, 8, 30, 0, DateTimeKind.Utc), 16, "Eren Hoca");
        SeedSession(db, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddd0128"), SportType.HIIT, new DateTime(y, m, 28, 19, 30, 0, DateTimeKind.Utc), 14, "Deniz Hoca");

        // ZUMBA (daily)
        // 22
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0022"), SportType.Zumba, new DateTime(y, m, 22, 12, 0, 0, DateTimeKind.Utc), 22, "Zeynep Hoca");
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0122"), SportType.Zumba, new DateTime(y, m, 22, 20, 0, 0, DateTimeKind.Utc), 28, "İrem Hoca");
        // 23
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0023"), SportType.Zumba, new DateTime(y, m, 23, 9, 0, 0, DateTimeKind.Utc), 20, "Zeynep Hoca");
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0123"), SportType.Zumba, new DateTime(y, m, 23, 18, 30, 0, DateTimeKind.Utc), 26, "İrem Hoca");
        // 24
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0024"), SportType.Zumba, new DateTime(y, m, 24, 11, 0, 0, DateTimeKind.Utc), 22, "Zeynep Hoca");
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0124"), SportType.Zumba, new DateTime(y, m, 24, 20, 0, 0, DateTimeKind.Utc), 28, "İrem Hoca");
        // 25
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0025"), SportType.Zumba, new DateTime(y, m, 25, 9, 30, 0, DateTimeKind.Utc), 20, "İrem Hoca");
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0125"), SportType.Zumba, new DateTime(y, m, 25, 19, 0, 0, DateTimeKind.Utc), 26, "Zeynep Hoca");
        // 26
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0026"), SportType.Zumba, new DateTime(y, m, 26, 12, 0, 0, DateTimeKind.Utc), 22, "Zeynep Hoca");
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0126"), SportType.Zumba, new DateTime(y, m, 26, 21, 0, 0, DateTimeKind.Utc), 30, "İrem Hoca");
        // 27
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0027"), SportType.Zumba, new DateTime(y, m, 27, 10, 0, 0, DateTimeKind.Utc), 22, "İrem Hoca");
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0127"), SportType.Zumba, new DateTime(y, m, 27, 18, 0, 0, DateTimeKind.Utc), 28, "Zeynep Hoca");
        // 28
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0028"), SportType.Zumba, new DateTime(y, m, 28, 12, 0, 0, DateTimeKind.Utc), 22, "Zeynep Hoca");
        SeedSession(db, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeee0128"), SportType.Zumba, new DateTime(y, m, 28, 20, 0, 0, DateTimeKind.Utc), 28, "İrem Hoca");

        // Extra Pilates (still fine if you want a bit more density)
        SeedSession(db, Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0024"), SportType.Pilates, new DateTime(y, m, 24, 20, 0, 0, DateTimeKind.Utc), 18, "Gizem Hoca");
        SeedSession(db, Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffff0026"), SportType.Pilates, new DateTime(y, m, 26, 8, 30, 0, DateTimeKind.Utc), 14, "Ece Hoca");

        db.SaveChanges();
        tx.Commit();
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
