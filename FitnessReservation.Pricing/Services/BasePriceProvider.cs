using FitnessReservation.Pricing.Models;
using System.Security.Cryptography.X509Certificates;

namespace FitnessReservation.Pricing.Services;
public sealed class BasePriceProvider
{
    public decimal GetBasePrice(SportType sport)
    {
        return sport switch
        {
            SportType.Yoga => 1250m,
            SportType.Pilates => 1280m,
            SportType.Spinning => 1260m,
            SportType.HIIT => 1270m,
            SportType.Zumba => 1240m,
            _ => throw new ArgumentOutOfRangeException(nameof(sport), sport, "Unknown sport type.")
        };
    }
}