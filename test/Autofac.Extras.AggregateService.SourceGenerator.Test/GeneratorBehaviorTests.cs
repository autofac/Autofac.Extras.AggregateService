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

        // No specific interface to name, so no AGSVC001 either.
        Assert.DoesNotContain(result.Diagnostics, d => d.Id == "AGSVC001");
    }

    [Fact]
    public void UnsupportedInterfaceReportsDiagnostic()
    {
        // An interface with a ref/out parameter cannot be generated; the generator should report
        // AGSVC001 so the silent dynamic-proxy fallback is visible to the consumer.
        var source = @"using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public interface IDep { }

    public interface IMyAggregate
    {
        IDep Get(out int value);
    }

    public static class Registration
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterAggregateService<IMyAggregate>();
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "AGSVC001");
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
        Assert.Contains("IMyAggregate", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void UnsupportedInterfaceReportedOncePerInterface()
    {
        // The same unsupported interface registered from multiple call sites should report only
        // a single AGSVC001 diagnostic.
        var source = @"using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public interface IDep { }

    public interface IMyAggregate
    {
        IDep Get(out int value);
    }

    public static class Registration
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterAggregateService<IMyAggregate>();
            builder.RegisterAggregateService(typeof(IMyAggregate));
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        Assert.Single(result.Diagnostics, d => d.Id == "AGSVC001");
    }

    [Fact]
    public void NonInterfaceTypeArgumentIsNotGenerated()
    {
        // CreateInstance<T> where T is not an interface cannot be an aggregate service; the
        // generator must skip it (the runtime throws ArgumentException instead).
        var source = @"using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public static class Registration
    {
        public static object Configure(IComponentContext context)
        {
            return AggregateServiceGenerator.CreateInstance<string>(context);
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void NestedInterfaceIsNotGenerated()
    {
        // An aggregate interface nested inside another type is not supported; the generator skips
        // it and the runtime falls back to the dynamic proxy.
        var source = @"using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public interface IDep { }

    public class Outer
    {
        public interface INested
        {
            IDep Dep { get; }
        }
    }

    public static class Registration
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterAggregateService<Outer.INested>();
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
        Assert.Single(result.Diagnostics, d => d.Id == "AGSVC001");
    }

    [Fact]
    public void InterfaceWithEventIsNotGenerated()
    {
        // Events are an unsupported member shape; the generator skips the interface and reports
        // AGSVC001 so the dynamic-proxy fallback is visible.
        var source = @"using System;
using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public interface IDep { }

    public interface IMyAggregate
    {
        IDep Dep { get; }

        event EventHandler Something;
    }

    public static class Registration
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterAggregateService<IMyAggregate>();
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
        Assert.Single(result.Diagnostics, d => d.Id == "AGSVC001");
    }

    [Fact]
    public void InterfaceWithNestedTypeMemberIsNotGenerated()
    {
        // A nested type is a member shape that is neither a property nor a method, so the
        // interface is unsupported and falls back to the dynamic proxy.
        var source = @"using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public interface IDep { }

    public interface IMyAggregate
    {
        IDep Dep { get; }

        public enum Mode
        {
            One,
        }
    }

    public static class Registration
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterAggregateService<IMyAggregate>();
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
        Assert.Single(result.Diagnostics, d => d.Id == "AGSVC001");
    }

    [Fact]
    public void InterfaceWithIndexerIsNotGenerated()
    {
        // An indexer can't be resolved as a single service; the interface falls back to the proxy.
        var source = @"using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public interface IDep { }

    public interface IMyAggregate
    {
        IDep this[int index] { get; }
    }

    public static class Registration
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterAggregateService<IMyAggregate>();
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
        Assert.Single(result.Diagnostics, d => d.Id == "AGSVC001");
    }

    [Fact]
    public void InterfaceWithWriteOnlyPropertyIsNotGenerated()
    {
        // A write-only property has nothing to resolve eagerly; the interface falls back.
        var source = @"using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public interface IDep { }

    public interface IMyAggregate
    {
        IDep WriteOnly { set; }
    }

    public static class Registration
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterAggregateService<IMyAggregate>();
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
        Assert.Single(result.Diagnostics, d => d.Id == "AGSVC001");
    }

    [Fact]
    public void NotNullConstraintIsGenerated()
    {
        // Exercises the 'notnull' generic constraint emission.
        var source = @"using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public interface IThing<T> { }

    public interface IMyAggregate
    {
        IThing<T> Get<T>() where T : notnull;
    }

    public static class Registration
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterAggregateService<IMyAggregate>();
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        var tree = Assert.Single(result.GeneratedTrees, t => t.FilePath.Contains("IMyAggregate", StringComparison.Ordinal));
        Assert.Contains("where T : notnull", tree.GetText().ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void NullConditionalInvocationIsDiscovered()
    {
        // A null-conditional registration call (builder?.RegisterAggregateService<T>()) uses a
        // MemberBindingExpressionSyntax; the generator must still discover it.
        var source = @"using Autofac;
using Autofac.Extras.AggregateService;

namespace TestConsumer
{
    public interface IDep { }

    public interface IMyAggregate
    {
        IDep Dep { get; }
    }

    public static class Registration
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder?.RegisterAggregateService<IMyAggregate>();
        }
    }
}";

        var driver = GeneratorTestHarness.Run(source);
        var result = driver.GetRunResult();

        Assert.Contains(result.GeneratedTrees, t => t.FilePath.Contains("IMyAggregate", StringComparison.Ordinal));
    }
}
