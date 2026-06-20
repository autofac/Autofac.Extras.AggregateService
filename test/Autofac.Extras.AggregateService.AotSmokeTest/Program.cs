// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Extras.AggregateService;
using Autofac.Extras.AggregateService.AotSmokeTest;

// Smoke test: resolve a source-generated aggregate service under NativeAOT/trimming and assert
// the wired-up dependencies come through. The aggregate is registered with a statically-visible
// RegisterAggregateService<T> call, so the generator emits a backing class and module initializer
// and this never touches the (non-AOT-safe) dynamic proxy fallback.
var builder = new ContainerBuilder();
builder.RegisterAggregateService<IMyAggregate>();
builder.RegisterType<FirstService>().As<IFirstService>();
builder.RegisterType<SecondService>().As<ISecondService>();

using var container = builder.Build();

var aggregate = container.Resolve<IMyAggregate>();

if (aggregate.First is null)
{
    Console.Error.WriteLine("FAIL: First property was not resolved.");
    return 1;
}

var second = aggregate.GetSecond();
if (second is null)
{
    Console.Error.WriteLine("FAIL: GetSecond() did not resolve a service.");
    return 1;
}

// Confirm the generated implementation was used, not the dynamic proxy fallback. The generated
// backing class lives in this assembly; the Castle proxy type would be in a dynamic assembly.
var implTypeName = aggregate.GetType().FullName ?? string.Empty;
if (!implTypeName.Contains("Aggregate", StringComparison.Ordinal) ||
    aggregate.GetType().Assembly != typeof(Program).Assembly)
{
    Console.Error.WriteLine($"FAIL: expected a generated implementation in this assembly but got '{implTypeName}'.");
    return 1;
}

Console.WriteLine($"PASS: resolved generated aggregate '{implTypeName}' with all dependencies.");
return 0;

/// <summary>
/// Marker partial class used to reference this assembly from top-level statements.
/// </summary>
internal sealed partial class Program
{
}
