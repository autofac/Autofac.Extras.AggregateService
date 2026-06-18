// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Autofac.Extras.AggregateService.SourceGenerator.Test;

public class GeneratorBehaviorTests
{
    [Fact]
    public void GeneratedSourcesCompileWithoutDiagnostics()
    {
        var source = @"using System;
using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public interface IThing<T> { }

    public interface IMyService { }

    public interface IClosedAggregate
    {
        IMyService Service { get; }
        IMyService GetService(int value);
        IThing<T> GetThing<T>();
        void DoNothing();
    }

    public interface IOpenAggregate<T>
    {
        T Item { get; }
    }

    public static class Registration
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterAggregateService<IClosedAggregate>();
            builder.RegisterAggregateService(typeof(IOpenAggregate<>));
        }
    }
}";

        var diagnostics = GeneratorTestHarness.GetResultingCompilationDiagnostics(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);
    }

    [Fact]
    public void LookAlikeMethodsAreNotMatched()
    {
        // A user type with methods named RegisterAggregateService / CreateInstance that are NOT
        // the Autofac ones must not trigger generation.
        var source = @"namespace TestConsumer
{
    public interface IMyAggregate { }

    public class NotAutofac
    {
        public void RegisterAggregateService<T>() { }

        public object CreateInstance<T>(object context) => null!;
    }

    public static class Registration
    {
        public static void Configure()
        {
            var fake = new NotAutofac();
            fake.RegisterAggregateService<IMyAggregate>();
            fake.CreateInstance<IMyAggregate>(new object());
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        // Only generated trees would be present if the generator matched; the lookalikes must
        // produce no generated output.
        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void RuntimeComputedTypeIsNotMatched()
    {
        // A registration whose Type is computed at runtime cannot be seen statically, so the
        // generator emits nothing and the runtime falls back to the dynamic proxy.
        var source = @"using System;
using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public static class Registration
    {
        public static void Configure(ContainerBuilder builder, Type runtimeType)
        {
            builder.RegisterAggregateService(runtimeType);
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
    }
}
