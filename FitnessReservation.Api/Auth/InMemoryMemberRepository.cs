namespace FitnessReservation.Api.Auth;

public sealed class InMemoryMemberRepository : IMemberRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Member> _byId = new();
    private readonly Dictionary<string, Guid> _byUsername = new(StringComparer.OrdinalIgnoreCase);

    public Member? FindByUsername(string username)
    {
        lock (_gate)
        {
            return _byUsername.TryGetValue(username, out var id)
                ? _byId[id]
                : null;
        }
    }

    public Member? Get(Guid memberId)
    {
        lock (_gate)
            return _byId.TryGetValue(memberId, out var m) ? m : null;
    }

    public void Add(Member member)
    {
        lock (_gate)
        {
            if (_byUsername.ContainsKey(member.Username))
                throw new InvalidOperationException("UsernameTaken");

            _byId[member.MemberId] = member;
            _byUsername[member.Username] = member.MemberId;
        }
    }
}
