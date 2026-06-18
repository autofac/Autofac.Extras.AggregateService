// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Autofac.Extras.AggregateService.Bench;

public sealed class Program
{
    private Program()
    {
    }

    public static void Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        // Usage:
        //
        // Run all benchmarks with the source code version of the project:
        // dotnet run -c Release --project bench/Autofac.Extras.AggregateService.Bench
        var config = new BenchmarkConfig();
        config.AddJob(Job.InProcess.WithId("Source"));

        new BenchmarkSwitcher(Benchmarks.All).Run(args, config);
    }
}
