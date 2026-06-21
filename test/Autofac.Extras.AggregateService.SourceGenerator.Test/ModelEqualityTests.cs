// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Autofac.Extras.AggregateService.SourceGenerator.Test;

/// <summary>
/// Verifies the value-equality of the incremental-pipeline model types. Structural
/// equality is what lets Roslyn cache generator outputs between runs, so equal
/// models must compare equal (and share a hash code) and any difference must
/// compare unequal.
/// </summary>
public class ModelEqualityTests
{
    [Fact]
    public void EquatableArray_EqualWhenElementsMatch()
    {
        var left = new EquatableArray<string>(new[] { "a", "b" });
        var right = new EquatableArray<string>(new[] { "a", "b" });

        Assert.True(left.Equals(right));
        Assert.True(left == right);
        Assert.False(left != right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void EquatableArray_UnequalWhenElementsDiffer()
    {
        var left = new EquatableArray<string>(new[] { "a", "b" });
        var right = new EquatableArray<string>(new[] { "a", "c" });

        Assert.False(left.Equals(right));
        Assert.True(left != right);
    }

    [Fact]
    public void EquatableArray_UnequalWhenLengthsDiffer()
    {
        var left = new EquatableArray<string>(new[] { "a" });
        var right = new EquatableArray<string>(new[] { "a", "b" });

        Assert.False(left.Equals(right));
    }

    [Fact]
    public void EquatableArray_DefaultAndDefaultAreEqual()
    {
        var left = default(EquatableArray<string>);
        var right = default(EquatableArray<string>);

        Assert.True(left.Equals(right));
        Assert.Empty(left);
        Assert.Equal(0, left.GetHashCode());
    }

    [Fact]
    public void EquatableArray_IndexerCountAndEnumeration()
    {
        var array = new EquatableArray<string>(new[] { "x", "y" });

        Assert.Equal(2, array.Count);
        Assert.Equal("x", array[0]);
        Assert.Equal(new[] { "x", "y" }, array.ToArray());
        Assert.Equal(new[] { "x", "y" }, System.Linq.Enumerable.ToArray(array));
    }

    [Fact]
    public void EquatableArray_EqualsObjectOverload()
    {
        var array = new EquatableArray<string>(new[] { "a" });

        Assert.True(array.Equals((object)new EquatableArray<string>(new[] { "a" })));
        Assert.False(array.Equals("not an array"));
        Assert.False(array.Equals(null));
    }

    [Fact]
    public void ParameterModel_EqualityConsidersAllMembers()
    {
        var baseline = new ParameterModel("value", "int", string.Empty);

        Assert.Equal(baseline, new ParameterModel("value", "int", string.Empty));
        Assert.Equal(baseline.GetHashCode(), new ParameterModel("value", "int", string.Empty).GetHashCode());
        Assert.NotEqual(baseline, new ParameterModel("other", "int", string.Empty));
        Assert.NotEqual(baseline, new ParameterModel("value", "string", string.Empty));
        Assert.NotEqual(baseline, new ParameterModel("value", "int", "in "));
        Assert.False(baseline.Equals("not a parameter"));
    }

    [Fact]
    public void TypeParameterModel_EqualityConsidersAllMembers()
    {
        var baseline = new TypeParameterModel("T", "class");

        Assert.Equal(baseline, new TypeParameterModel("T", "class"));
        Assert.Equal(baseline.GetHashCode(), new TypeParameterModel("T", "class").GetHashCode());
        Assert.NotEqual(baseline, new TypeParameterModel("U", "class"));
        Assert.NotEqual(baseline, new TypeParameterModel("T", "struct"));
        Assert.False(baseline.Equals("not a type parameter"));
    }

    [Fact]
    public void MemberModel_EqualityConsidersAllMembers()
    {
        var baseline = CreateMember();

        Assert.Equal(baseline, CreateMember());
        Assert.Equal(baseline.GetHashCode(), CreateMember().GetHashCode());
        Assert.NotEqual(baseline, CreateMember(name: "Other"));
        Assert.NotEqual(baseline, CreateMember(kind: MemberKind.ParameterlessMethod));
        Assert.NotEqual(baseline, CreateMember(returnType: "string"));
        Assert.NotEqual(baseline, CreateMember(hasSetter: true));
        Assert.False(baseline.Equals("not a member"));
    }

    [Fact]
    public void AggregateServiceModel_EqualityConsidersAllMembers()
    {
        var baseline = CreateAggregate();

        Assert.Equal(baseline, CreateAggregate());
        Assert.Equal(baseline.GetHashCode(), CreateAggregate().GetHashCode());
        Assert.NotEqual(baseline, CreateAggregate(name: "global::Other.IFoo"));
        Assert.NotEqual(baseline, CreateAggregate(isOpenGeneric: true));
        Assert.False(baseline.Equals("not an aggregate"));
    }

    [Fact]
    public void DiscoveryResult_SupportedAndUnsupportedAreDistinct()
    {
        var supported = DiscoveryResult.Supported(CreateAggregate());
        var unsupported = DiscoveryResult.Unsupported("global::Foo.IBar", location: null);

        Assert.Equal(supported, DiscoveryResult.Supported(CreateAggregate()));
        Assert.Equal(unsupported, DiscoveryResult.Unsupported("global::Foo.IBar", location: null));
        Assert.NotEqual(supported, unsupported);
        Assert.NotEqual(unsupported, DiscoveryResult.Unsupported("global::Foo.IOther", location: null));
        Assert.Equal(supported.GetHashCode(), DiscoveryResult.Supported(CreateAggregate()).GetHashCode());
        Assert.False(supported.Equals("not a result"));
    }

    [Fact]
    public void LocationInfo_RoundTripsAndCompares()
    {
        var span = new TextSpan(0, 5);
        var lineSpan = new LinePositionSpan(new LinePosition(1, 0), new LinePosition(1, 5));
        var baseline = new LocationInfo("File.cs", span, lineSpan);

        Assert.Equal(baseline, new LocationInfo("File.cs", span, lineSpan));
        Assert.Equal(baseline.GetHashCode(), new LocationInfo("File.cs", span, lineSpan).GetHashCode());
        Assert.NotEqual(baseline, new LocationInfo("Other.cs", span, lineSpan));
        Assert.False(baseline.Equals("not a location"));

        var location = baseline.ToLocation();
        Assert.Equal("File.cs", location.GetLineSpan().Path);
    }

    private static MemberModel CreateMember(
        MemberKind kind = MemberKind.Property,
        string name = "Service",
        string returnType = "global::Foo.IService",
        bool hasSetter = false)
        => new MemberModel(
            kind,
            name,
            returnType,
            new EquatableArray<ParameterModel>(System.Array.Empty<ParameterModel>()),
            new EquatableArray<TypeParameterModel>(System.Array.Empty<TypeParameterModel>()),
            isGenericMethod: false,
            hasSetter: hasSetter);

    private static AggregateServiceModel CreateAggregate(
        string name = "global::Foo.IBar",
        bool isOpenGeneric = false)
        => new AggregateServiceModel(
            interfaceFullyQualifiedName: name,
            interfaceNamespace: "Foo",
            interfaceMinimalName: "IBar",
            backingClassName: "__Foo_IBar_Aggregate",
            isOpenGeneric: isOpenGeneric,
            typeParameters: new EquatableArray<TypeParameterModel>(System.Array.Empty<TypeParameterModel>()),
            members: new EquatableArray<MemberModel>(System.Array.Empty<MemberModel>()));
}
