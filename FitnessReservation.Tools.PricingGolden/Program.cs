using System.Globalization;
using FitnessReservation.Pricing.Models;
using FitnessReservation.Pricing.Services;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: PricingGolden <inputCsv> <outputCsv>");
    return;
}

var inputPath = Path.GetFullPath(args[0]);
var outputPath = Path.GetFullPath(args[1]);

if (!File.Exists(inputPath))
{
    Console.Error.WriteLine($"Input not found: {inputPath}");
    return;
}

var engine = new PricingEngine(new BasePriceProvider(), new MultiplierProvider());

var lines = File.ReadAllLines(inputPath);

// Expect header: CaseId,Sport,Membership,IsPeak,Occupancy
if (lines.Length == 0)
{
    Console.Error.WriteLine("Input CSV is empty.");
    return;
}

var header = lines[0].Trim();
if (!header.Equals("CaseId,Sport,Membership,IsPeak,Occupancy", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine($"Unexpected header: {header}");
    return;
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

using var writer = new StreamWriter(outputPath, false);
writer.WriteLine("CaseId,Sport,Membership,IsPeak,Occupancy,ExpectedFinalPrice");

for (int i = 1; i < lines.Length; i++)
{
    var line = lines[i].Trim();
    if (string.IsNullOrWhiteSpace(line)) continue;

    var parts = line.Split(',');
    if (parts.Length != 5)
    {
        Console.Error.WriteLine($"Bad row (expected 5 columns) at line {i + 1}: {line}");
        return;
    }

    var caseId = parts[0];
    var sport = Enum.Parse<SportType>(parts[1], ignoreCase: true);
    var membership = Enum.Parse<MembershipType>(parts[2], ignoreCase: true);
    var isPeak = bool.Parse(parts[3]);
    var occupancy = Enum.Parse<OccupancyLevel>(parts[4], ignoreCase: true);

    var request = new PricingRequest
    {
        Sport = sport,
        Membership = membership,
        IsPeak = isPeak,
        Occupancy = occupancy
    };

    var result = engine.Calculate(request);

    // IMPORTANT: keep '.' in CSV regardless of Turkish locale
    var expected = result.FinalPrice.ToString(CultureInfo.InvariantCulture);

    writer.WriteLine($"{caseId},{parts[1]},{parts[2]},{parts[3]},{parts[4]},{expected}");
}

Console.WriteLine($"Wrote: {outputPath}");
