using System.Diagnostics;

namespace MDict.Csharp.Utils;

/// <summary>
/// Example usage:
/// <code>
/// var mem = new MemoryMeasurer();
/// mem.Measure("Start");
/// 
/// var timedFn = PerformanceUtils.MeasureTimeFn(() =>
/// 
/// {
///     // Simulate work
///     System.Threading.Thread.Sleep(300);
/// }, "Sleep300");
/// 
/// timedFn();
/// 
/// mem.Measure("After Sleep");
/// 
/// var timedCalc = PerformanceUtils.MeasureTimeFn(() =>
/// {
///     return Enumerable.Range(0, 1000000).Sum(x => x);
/// }, "SumCalc");
/// 
/// var result = timedCalc();
/// Console.WriteLine($"Sum Result: {result}");
/// 
/// mem.Measure("After Calculation");
/// </code>
/// </summary>
internal static class MeasureUtils
{
    /// <summary>
    /// Measure the time cost of a function call (generic version with return value).
    /// </summary>
    /// <typeparam name="T">Return type of the function</typeparam>
    /// <param name="fn">The callee function</param>
    /// <param name="name">Function name (optional)</param>
    /// <returns>A wrapped function that logs the time taken</returns>
    public static Func<T> MeasureTimeFn<T>(Func<T> fn, string name = "unknown")
    {
        return () =>
        {
            var stopwatch = Stopwatch.StartNew();
            T result = fn();
            stopwatch.Stop();
            Console.WriteLine($"{name} took {stopwatch.ElapsedMilliseconds}ms");
            return result;
        };
    }

    /// <summary>
    /// Measure the time cost of a function call (void version).
    /// </summary>
    /// <param name="fn">The callee action</param>
    /// <param name="name">Function name (optional)</param>
    /// <returns>A wrapped action that logs the time taken</returns>
    public static Action MeasureTimeFn(Action fn, string name = "unknown")
    {
        return () =>
        {
            var stopwatch = Stopwatch.StartNew();
            fn();
            stopwatch.Stop();
            Console.WriteLine($"{name} took {stopwatch.ElapsedMilliseconds}ms");
        };
    }

    private static readonly List<Process> _snapshots = new();
    private static int _step = -1;

    /// <summary>
    /// Measure memory usage at the current step and print the memory usage
    /// including differences from the previous measurement.
    /// </summary>
    /// <param name="category">The label or category name for this measurement step</param>
    public static void Measure(string category)
    {
        _step++;
        var process = Process.GetCurrentProcess();
        _snapshots.Add(process);

        long workingSet = process.WorkingSet64;
        long privateMemory = process.PrivateMemorySize64;
        long virtualMemory = process.VirtualMemorySize64;

        long lastWorkingSet = _step > 0 ? _snapshots[_step - 1].WorkingSet64 : 0;
        long lastPrivateMemory = _step > 0 ? _snapshots[_step - 1].PrivateMemorySize64 : 0;
        long lastVirtualMemory = _step > 0 ? _snapshots[_step - 1].VirtualMemorySize64 : 0;

        Console.WriteLine($"Step {_step} - Category: {category}");
        Console.WriteLine($"{"Key",-20} {"Used(MB)",10} {"Diff(MB)",10}");
        Console.WriteLine($"{"WorkingSet",-20} {ToMB(workingSet),10:F2} {ToMB(workingSet - lastWorkingSet),10:F2}");
        Console.WriteLine($"{"PrivateMemory",-20} {ToMB(privateMemory),10:F2} {ToMB(privateMemory - lastPrivateMemory),10:F2}");
        Console.WriteLine($"{"VirtualMemory",-20} {ToMB(virtualMemory),10:F2} {ToMB(virtualMemory - lastVirtualMemory),10:F2}");
        Console.WriteLine();
    }

    private static double ToMB(long bytes) => Math.Round(bytes / 1024.0 / 1024.0, 2);
}
