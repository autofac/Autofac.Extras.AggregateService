// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Extras.AggregateService;
using Autofac.Extras.AggregateService.AotSmokeTest;

// Smoke test: resolve source-generated aggregate services under NativeAOT/trimming and assert
// the wired-up dependencies come through. Every aggregate here is registered with a
// statically-visible call so the generator emits backing classes and a module initializer;
// none of this touches the (non-AOT-safe) dynamic proxy fallback. Returns 0 on success and a
// non-zero code (with a message) on the first failure, so CI can assert both that the publish
// is warning-clean AND that the generated code actually runs correctly under AOT.
// NOTE: only CLOSED aggregate services are exercised here. Open generic aggregates require
// MakeGenericType + constructing a runtime-determined closed type, which is inherently not
// NativeAOT-compatible (it throws under AOT); that is a documented limitation, so it is
// deliberately excluded from this "must run under AOT" smoke test.
var builder = new ContainerBuilder();
builder.RegisterAggregateService<IMyAggregate>();
builder.RegisterType<FirstService>().As<IFirstService>().InstancePerLifetimeScope();
builder.RegisterType<SecondService>().As<ISecondService>();

using var container = builder.Build();

var aggregate = container.Resolve<IMyAggregate>();

// 1. Eagerly-resolved property.
if (aggregate.First is null)
{
    return Fail("First property was not resolved.");
}

// 2. Parameterless resolving method.
if (aggregate.GetSecond() is null)
{
    return Fail("GetSecond() did not resolve a service.");
}

// 3. Resolving method with a parameter (the generated TypedParameter path).
if (aggregate.GetSecond(42) is null)
{
    return Fail("GetSecond(int) did not resolve a service.");
}

// 4. Confirm the generated implementation was used, not the dynamic proxy fallback. The
// generated backing class lives in this assembly; a Castle proxy would be in a dynamic assembly.
var implTypeName = aggregate.GetType().FullName ?? string.Empty;
if (!implTypeName.Contains("Aggregate", StringComparison.Ordinal) ||
    aggregate.GetType().Assembly != typeof(Program).Assembly)
{
    return Fail($"expected a generated implementation in this assembly but got '{implTypeName}'.");
}

// 5. Resolve from a child lifetime scope and confirm the scoped dependency is honored.
using (var scope = container.BeginLifetimeScope())
{
    var scopedAggregate = scope.Resolve<IMyAggregate>();
    var scopedFirst = scope.Resolve<IFirstService>();
    if (!ReferenceEquals(scopedAggregate.First, scopedFirst))
    {
        return Fail("aggregate resolved from a child scope did not use the scoped dependency.");
    }
}

Console.WriteLine($"PASS: resolved generated aggregates (including '{implTypeName}') with all dependencies.");
return 0;

static int Fail(string message)
{
    Console.Error.WriteLine($"FAIL: {message}");
    return 1;
}

/// <summary>
/// Marker partial class used to reference this assembly from top-level statements.
/// </summary>
internal sealed partial class Program
{
}
