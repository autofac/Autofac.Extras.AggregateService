// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.CompilerServices;

namespace Autofac.Extras.AggregateService.SourceGenerator.Test;

/// <summary>
/// Configures Verify for source generator snapshot testing.
/// </summary>
internal static class VerifyModuleInitializer
{
    /// <summary>
    /// Initializes Verify's source generator support before any tests run.
    /// </summary>
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifySourceGenerators.Initialize();

        // Route all snapshot files into a "Snapshots" folder beside the test
        // source rather than scattering them across the project root.
        DerivePathInfo((sourceFile, _, type, method) =>
            new PathInfo(
                directory: Path.Combine(Path.GetDirectoryName(sourceFile)!, "Snapshots"),
                typeName: type.Name,
                methodName: method.Name));
    }
}
