// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using BenchmarkDotNet.Running;

namespace Autofac.Extras.AggregateService.Bench;

public class Harness
{
    [Fact]
    public void AggregateService()
    {
        var exception = Record.Exception(RunBenchmark<AggregateServiceBenchmark>);
        Assert.Null(exception);
    }

    /// <remarks>
    /// This method enforces that benchmark types are added to <see cref="Benchmarks.All"/>
    /// so that they can be used directly from the command line in <see cref="Program.Main"/> as well.
    /// </remarks>
    private static void RunBenchmark<TBenchmark>()
    {
        var targetType = typeof(TBenchmark);
        var benchmarkType = Benchmarks.All.Single(type => type == targetType);
        BenchmarkRunner.Run(benchmarkType, new BenchmarkConfig());
    }
}
