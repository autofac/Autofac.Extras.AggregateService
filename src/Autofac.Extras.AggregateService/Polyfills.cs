// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// Polyfills for trimming / NativeAOT annotation attributes and nullable analysis
// attributes that do not exist on the older target frameworks this package supports.
// These are declared internal so they never become part of the public surface; the
// compiler embeds them as ordinary attribute usages, which the modern linker still reads
// when the package is consumed from a trimming/AOT-enabled net8.0+ application.
//
// Every attribute defined here exists in-box on net8.0+, so the polyfills are only needed
// for the netstandard target frameworks. Guarding the whole file keeps the namespace from
// being empty (and tripping the empty-namespace analyzer) on the modern targets.
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

#if NETSTANDARD2_0 || NETSTANDARD2_1
namespace System.Diagnostics.CodeAnalysis;

#if !NETSTANDARD2_1_OR_GREATER
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class NotNullWhenAttribute : Attribute
{
    public NotNullWhenAttribute(bool returnValue)
    {
        ReturnValue = returnValue;
    }

    public bool ReturnValue
    {
        get;
    }
}
#endif

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class RequiresUnreferencedCodeAttribute : Attribute
{
    public RequiresUnreferencedCodeAttribute(string message)
    {
        Message = message;
    }

    public string Message
    {
        get;
    }

    public string? Url
    {
        get; set;
    }
}

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
internal sealed class UnconditionalSuppressMessageAttribute : Attribute
{
    public UnconditionalSuppressMessageAttribute(string category, string checkId)
    {
        Category = category;
        CheckId = checkId;
    }

    public string Category
    {
        get;
    }

    public string CheckId
    {
        get;
    }

    public string? Scope
    {
        get; set;
    }

    public string? Target
    {
        get; set;
    }

    public string? MessageId
    {
        get; set;
    }

    public string? Justification
    {
        get; set;
    }
}

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class RequiresDynamicCodeAttribute : Attribute
{
    public RequiresDynamicCodeAttribute(string message)
    {
        Message = message;
    }

    public string Message
    {
        get;
    }

    public string? Url
    {
        get; set;
    }
}
#endif
