// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Bench;

/// <summary>
/// The set of benchmark types that can be run from the command line or exercised by the test harness.
/// </summary>
public static class Benchmarks
{
    /// <summary>
    /// All benchmark types in this assembly.
    /// </summary>
    public static readonly Type[] All =
    {
        typeof(AggregateServiceBenchmark),
    };
}
