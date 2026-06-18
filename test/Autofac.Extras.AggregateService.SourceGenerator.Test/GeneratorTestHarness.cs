// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Autofac.Extras.AggregateService.SourceGenerator.Test;

/// <summary>
/// Helpers for running the <see cref="AggregateServiceSourceGenerator"/> over a snippet of
/// source code so the emitted output and diagnostics can be asserted.
/// </summary>
internal static class GeneratorTestHarness
{
    private static readonly ImmutableArray<MetadataReference> _referenceAssemblies = BuildReferences();

    /// <summary>
    /// Runs the generator against the supplied source and returns the resulting driver so the
    /// caller can verify generated trees and diagnostics.
    /// </summary>
    /// <param name="source">
    /// The C# source to compile and run the generator against.
    /// </param>
    /// <returns>
    /// The generator driver after a single generation pass.
    /// </returns>
    public static GeneratorDriver Run(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: "AggregateServiceGeneratorTestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: _referenceAssemblies,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new AggregateServiceSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }

    /// <summary>
    /// Runs the generator and returns the diagnostics produced by compiling the original source
    /// together with the generated output. Used to assert the generated code compiles cleanly.
    /// </summary>
    /// <param name="source">
    /// The C# source to compile and run the generator against.
    /// </param>
    /// <returns>
    /// The diagnostics from the final compilation including generated sources.
    /// </returns>
    public static ImmutableArray<Diagnostic> GetResultingCompilationDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: "AggregateServiceGeneratorTestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: _referenceAssemblies,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new AggregateServiceSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        return outputCompilation.GetDiagnostics();
    }

    private static ImmutableArray<MetadataReference> BuildReferences()
    {
        var builder = ImmutableArray.CreateBuilder<MetadataReference>();

        // The core framework assemblies for the running test framework.
        var trustedAssemblies = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)?
            .Split(Path.PathSeparator) ?? Array.Empty<string>();
        foreach (var path in trustedAssemblies)
        {
            builder.Add(MetadataReference.CreateFromFile(path));
        }

        // Autofac and the aggregate service runtime so the call sites and registry resolve.
        builder.Add(MetadataReference.CreateFromFile(typeof(ContainerBuilder).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(GeneratedAggregateServiceRegistry).Assembly.Location));

        return builder.ToImmutable();
    }
}
