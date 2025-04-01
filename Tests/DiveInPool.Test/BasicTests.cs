using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DiveIndexPool;
using DotNext.Collections.Concurrent; // Assuming you have a reference to DotNext

namespace DiveIndexPool.Benchmarks;

/// <summary>
/// Benchmarks comparing our custom IndexPool against the DotNext implementation.
/// Note: The DotNext version supports up to 64 indices.
/// </summary>
public class IndexPoolComparisonBenchmark
{
    // Our implementation with a larger capacity.
    private IndexPool<int> _ourPool;

    // DotNext implementation (non-generic) which supports up to 64 indices.
    private DotNext.Collections.Concurrent.IndexPool _dotNextPool;

    [GlobalSetup]
    public void Setup()
    {
        // Create our index pool with a capacity of 1000 indices.
        _ourPool = IndexPool<int>.Create(1000);

        // Create the DotNext index pool.
        // Note: DotNext's implementation has a fixed capacity of 64 (0..63)
        _dotNextPool = new DotNext.Collections.Concurrent.IndexPool(63);
    }

    [Benchmark]
    public int OurPool_Take()
    {
        // Benchmark the take operation from our pool.
        _ourPool.TryTake(out int index);
        return index;
    }

    [Benchmark]
    public int DotNextPool_Take()
    {
        // Benchmark the take operation from the DotNext pool.
        _dotNextPool.TryTake(out int index);
        return index;
    }

    [Benchmark]
    public void OurPool_Return()
    {
        // Take and then return an index from our pool.
        if (_ourPool.TryTake(out int index))
        {
            _ourPool.Return(index);
        }
    }

    [Benchmark]
    public void DotNextPool_Return()
    {
        // Take and then return an index from the DotNext pool.
        if (_dotNextPool.TryTake(out int index))
        {
            _dotNextPool.Return(index);
        }
    }
}

internal static class Program
{
    private static void Main(string[] args)
    {
        // Run the benchmarks.
        BenchmarkRunner.Run<IndexPoolComparisonBenchmark>();
    }
}
