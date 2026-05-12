namespace Karakol.Performance.Tests;

internal static class LargeLogFixture
{
    public static async Task<string> CreateAsync(int lineCount)
    {
        var path = Path.Combine(Path.GetTempPath(), "karakol-performance-tests", $"{lineCount}-{Guid.NewGuid():N}.log");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var stream = File.Create(path);
        await using var writer = new StreamWriter(stream);
        for (var index = 0; index < lineCount; index++)
        {
            await writer.WriteLineAsync($"127.0.0.1 - - [10/Oct/2026:13:55:36 +0300] \"GET /home/{index} HTTP/1.1\" 200 2326 \"-\" \"Mozilla/5.0\"");
        }

        return path;
    }
}
