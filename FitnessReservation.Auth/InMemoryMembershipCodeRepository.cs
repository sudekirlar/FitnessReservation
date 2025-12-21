namespace FitnessReservation.Api.Auth;

public sealed class InMemoryMembershipCodeRepository : IMembershipCodeRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<string, MembershipCode> _codes = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryMembershipCodeRepository(IEnumerable<MembershipCode> seed)
    {
        foreach (var c in seed)
            _codes[c.Code] = c;
    }

    public MembershipCode? Get(string code)
    {
        lock (_gate)
            return _codes.TryGetValue(code, out var c) ? c : null;
    }

    public void MarkUsed(string code, Guid memberId)
    {
        lock (_gate)
        {
            if (!_codes.TryGetValue(code, out var c))
                throw new InvalidOperationException("CodeNotFound");

            c.UsedByMemberId = memberId;
        }
    }
}
