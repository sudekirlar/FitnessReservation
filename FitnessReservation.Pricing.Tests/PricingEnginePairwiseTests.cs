using System.Globalization;
using FluentAssertions;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;
using Xunit;

namespace FitnessReservation.Pricing.Tests;

public class PricingEnginePairwiseTests
{
    public static IEnumerable<object[]> Cases()
    {
        var root = TestDataPaths.FindRepoRoot();
        var path = Path.Combine(root, "testdata", "pricing", "pairwise-cases.expected.csv");

        var lines = File.ReadAllLines(path);
        // header: CaseId,Sport,Membership,IsPeak,Occupancy,ExpectedFinalPrice
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var p = line.Split(',');
            yield return new object[]
            {
                p[0], // CaseId
                Enum.Parse<SportType>(p[1], true),
                Enum.Parse<MembershipType>(p[2], true),
                bool.Parse(p[3]),
                Enum.Parse<OccupancyLevel>(p[4], true),
                decimal.Parse(p[5], CultureInfo.InvariantCulture)
            };
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Calculate_ShouldMatchGoldenExpected(
        string caseId,
        SportType sport,
        MembershipType membership,
        bool isPeak,
        OccupancyLevel occupancy,
        decimal expectedFinalPrice)
    {   
        // Act
        var engine = new PricingEngine(new BasePriceProvider(), new MultiplierProvider());

        var request = new PricingRequest
        {
            Sport = sport,
            Membership = membership,
            IsPeak = isPeak,
            Occupancy = occupancy
        };

        // Arrange
        var result = engine.Calculate(request);

        // Assert
        result.FinalPrice.Should().Be(expectedFinalPrice, because: caseId);
    }
}
