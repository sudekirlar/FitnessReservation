namespace FitnessReservation.Api.Auth;

public static class AuthSession
{
    private const string MemberIdKey = "memberId";

    public static void SignIn(HttpContext ctx, Guid memberId)
        => ctx.Session.SetString(MemberIdKey, memberId.ToString());

    public static void SignOut(HttpContext ctx)
        => ctx.Session.Remove(MemberIdKey);

    public static bool TryGetMemberId(HttpContext ctx, out Guid memberId)
    {
        memberId = default;
        var s = ctx.Session.GetString(MemberIdKey);
        return Guid.TryParse(s, out memberId);
    }
}
