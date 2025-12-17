namespace FitnessReservation.Pricing.Tests;

internal static class TestDataPaths
{
    public static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var testdata = Path.Combine(dir.FullName, "testdata");
            if (Directory.Exists(testdata))
                return dir.FullName;

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root (testdata folder not found).");
    }
}
