using FitnessReservation.Pricing.Models;
using System.Security.Cryptography.X509Certificates;

namespace FitnessReservation.Pricing.Services
{
    public class BasePriceProvider
    {
        public decimal GetBasePrice(SportType sport)
        {
            throw new NotImplementedException();
        }
    }
}