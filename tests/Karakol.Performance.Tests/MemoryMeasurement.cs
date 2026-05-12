namespace Karakol.Performance.Tests;

internal static class MemoryMeasurement
{
    public static long MeasureAllocatedBytes(Action action)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetTotalMemory(forceFullCollection: true);
        action();
        var after = GC.GetTotalMemory(forceFullCollection: true);
        return Math.Max(0, after - before);
    }

    public static async Task<long> MeasureAllocatedBytesAsync(Func<Task> action)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetTotalMemory(forceFullCollection: true);
        await action().ConfigureAwait(false);
        var after = GC.GetTotalMemory(forceFullCollection: true);
        return Math.Max(0, after - before);
    }
}
