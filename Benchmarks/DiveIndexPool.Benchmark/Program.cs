using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using DotNext.Collections.Concurrent;
using DiveIndexPool;
using BenchmarkDotNet.Engines;

namespace DiveIndexPool.Benchmarks;

[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 3,
                                   iterationCount: 10)]
[MemoryDiagnoser]
public class OurIndexPoolBenchmarks
{
    [Params(63, 1024, 65535)]
    public int Capacity;

    private IIndexPool<int> _Pool = null!;
    private int[] _BatchIndices   = null!;

    [GlobalSetup]
    public void Setup()
    {
        _Pool = IndexPool.Create<int>((ulong)Capacity);

        int batchSize = Math.Min(Capacity, 64);
        _BatchIndices = new int[batchSize];

        for (int i = 0; i < batchSize; i++)
        {
            _BatchIndices[i] = i;
        }
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public void TakeTest()
    {
        for (int i = 0; i < 1000; i++)
        {
            _Pool.TryTake(out _);
        }
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public void ReturnTest()
    {
        for (int i = 0; i < 1000; i++)
        {
            if (_Pool.TryTake(out int index))
                _Pool.Return(index);
        }
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public void ResetTest()
    {
        for (int i = 0; i < 1000; i++)
            _Pool.Reset();
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public void EnumerateTest()
    {
        for (int i = 0; i < 1000; i++)
        {
            foreach (int index in _Pool)
            {
                //
            }
        }
    }

    [Benchmark(OperationsPerInvoke = 500)]
    public void BatchReturnTest()
    {

        for (int i = 0; i < 500; i++)
        {
            _Pool.ReturnAll(_BatchIndices);
        }
    }

    [Benchmark(OperationsPerInvoke = 500)]
    public void RealisticUsageTest()
    {
        for (int i = 0; i < 500; i++)
        {
            if (_Pool.TryTake(out int index))
            {
                _ = index * 2;

                _Pool.Return(index);
            }
        }
    }
}

[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[MemoryDiagnoser]
public class DotNextIndexPoolBenchmarks
{
    [Params(63)]
    public int Capacity;

    private DotNext.Collections.Concurrent.IndexPool _DotNextPool;
    private int[] _BatchIndices = null!;

    [GlobalSetup]
    public void Setup()
    {
        _DotNextPool = new DotNext.Collections.Concurrent.IndexPool(Capacity);
        _BatchIndices = new int[Math.Min(Capacity, 64)];

        for (int i = 0; i < _BatchIndices.Length; i++)
            _BatchIndices[i] = i;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public void TakeTest()
    {
        for (int i = 0; i < 1000; i++)
        {
            _DotNextPool.TryTake(out _);
        }
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public void ReturnTest()
    {
        for (int i = 0; i < 1000; i++)
        {
            if (_DotNextPool.TryTake(out int index))
                _DotNextPool.Return(index);
        }
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public void ResetTest()
    {
        for (int i = 0; i < 1000; i++)
            _DotNextPool.Reset();
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public void EnumerateTest()
    {
        for (int i = 0; i < 1000; i++)
        {
            foreach (int _ in _DotNextPool)
            {
                // No-op
            }
        }
    }

    [Benchmark(OperationsPerInvoke = 500)]
    public void BatchReturnTest()
    {
        for (int i = 0; i < 500; i++)
        {
            foreach (int index in _BatchIndices)
                _DotNextPool.Return(index);
        }
    }

    [Benchmark(OperationsPerInvoke = 500)]
    public void RealisticUsageTest()
    {
        for (int i = 0; i < 500; i++)
        {
            if (_DotNextPool.TryTake(out int index))
            {
                int _ = index * 2;
                _DotNextPool.Return(index);
            }
        }
    }
}

internal static class Program
{
    public static void Main(string[] _)
    {
        BenchmarkRunner.Run<OurIndexPoolBenchmarks>();
        BenchmarkRunner.Run<DotNextIndexPoolBenchmarks>();
    }
}
