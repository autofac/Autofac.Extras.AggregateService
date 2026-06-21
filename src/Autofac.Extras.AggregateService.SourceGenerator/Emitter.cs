// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// Emits the C# source for generated aggregate service backing classes and the
/// registry registration module initializer.
/// </summary>
internal static class Emitter
{
    private const string GeneratedCodeAttribute =
        "[global::System.CodeDom.Compiler.GeneratedCode(\"Autofac.Extras.AggregateService.SourceGenerator\", null)]";

    private const string ContextField = "__autofacContext";

    private const string PublicModifier = "public ";

    /// <summary>
    /// Emits a backing class that implements a single aggregate service
    /// interface.
    /// </summary>
    /// <param name="model">
    /// The interface model to implement.
    /// </param>
    /// <returns>
    /// The generated C# source.
    /// </returns>
    public static string EmitBackingClass(AggregateServiceModel model)
    {
        var sb = new StringBuilder();
        AppendHeader(sb);

        var hasNamespace = !string.IsNullOrEmpty(model.InterfaceNamespace);
        var indent = hasNamespace ? "    " : string.Empty;

        if (hasNamespace)
        {
            sb.Append("namespace ").Append(model.InterfaceNamespace).AppendLine();
            sb.AppendLine("{");
        }

        var typeParameterList = BuildTypeParameterList(model.TypeParameters);
        var interfaceRef = BuildInterfaceReference(model);

        sb.Append(indent).AppendLine(GeneratedCodeAttribute);
        sb.Append(indent)
            .Append("internal sealed class ")
            .Append(model.BackingClassName)
            .Append(typeParameterList)
            .Append(" : ")
            .Append(interfaceRef)
            .Append(BuildConstraintClauses(model.TypeParameters))
            .AppendLine();
        sb.Append(indent).AppendLine("{");

        var bodyIndent = indent + "    ";

        // Context field.
        sb.Append(bodyIndent)
            .Append("private readonly global::Autofac.IComponentContext ")
            .Append(ContextField)
            .AppendLine(";");
        sb.AppendLine();

        // Backing fields for eagerly-resolved properties.
        foreach (var member in model.Members)
        {
            if (member.Kind == MemberKind.Property)
            {
                sb.Append(bodyIndent)
                    .Append("private readonly ")
                    .Append(member.ReturnType)
                    .Append(' ')
                    .Append(BackingFieldName(member.Name))
                    .AppendLine(";");
            }
        }

        sb.AppendLine();

        // Constructor: store context and resolve all properties eagerly.
        sb.Append(bodyIndent)
            .Append(PublicModifier)
            .Append(model.BackingClassName)
            .AppendLine("(global::Autofac.IComponentContext context)");
        sb.Append(bodyIndent).AppendLine("{");
        sb.Append(bodyIndent).Append("    ").Append(ContextField).AppendLine(" = context;");
        foreach (var member in model.Members)
        {
            if (member.Kind == MemberKind.Property)
            {
                sb.Append(bodyIndent)
                    .Append("    ")
                    .Append(BackingFieldName(member.Name))
                    .Append(" = (")
                    .Append(member.ReturnType)
                    .Append(")global::Autofac.ResolutionExtensions.Resolve(")
                    .Append(ContextField)
                    .Append(", typeof(")
                    .Append(member.ReturnType)
                    .AppendLine("));");
            }
        }

        sb.Append(bodyIndent).AppendLine("}");

        // Members.
        foreach (var member in model.Members)
        {
            sb.AppendLine();
            EmitMember(sb, bodyIndent, member);
        }

        sb.Append(indent).AppendLine("}");

        if (hasNamespace)
        {
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Emits the module initializer that registers every generated backing
    /// class with the runtime registry.
    /// </summary>
    /// <param name="models">
    /// The interface models that were generated.
    /// </param>
    /// <param name="needsModuleInitializerPolyfill">
    /// Whether to emit a <c>ModuleInitializerAttribute</c> polyfill.
    /// </param>
    /// <returns>
    /// The generated C# source.
    /// </returns>
    public static string EmitRegistrations(IReadOnlyList<AggregateServiceModel> models, bool needsModuleInitializerPolyfill)
    {
        var sb = new StringBuilder();
        AppendHeader(sb);

        if (needsModuleInitializerPolyfill)
        {
            sb.AppendLine("namespace System.Runtime.CompilerServices");
            sb.AppendLine("{");
            sb.AppendLine("    [global::System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false)]");
            sb.AppendLine("    internal sealed class ModuleInitializerAttribute : global::System.Attribute");
            sb.AppendLine("    {");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        sb.AppendLine("namespace Autofac.Extras.AggregateService.Generated");
        sb.AppendLine("{");
        sb.Append("    ").AppendLine(GeneratedCodeAttribute);
        sb.AppendLine("    internal static class GeneratedAggregateServiceModuleInitializer");
        sb.AppendLine("    {");
        sb.AppendLine("        [global::System.Runtime.CompilerServices.ModuleInitializer]");
        sb.AppendLine("        internal static void Initialize()");
        sb.AppendLine("        {");

        foreach (var model in models)
        {
            if (model.IsOpenGeneric)
            {
                sb.Append("            global::Autofac.Extras.AggregateService.GeneratedAggregateServiceRegistry.RegisterOpenGeneric(typeof(")
                    .Append(OpenTypeofExpression(model))
                    .Append("), typeof(global::")
                    .Append(QualifiedBackingClass(model))
                    .Append(OpenBackingTypeofSuffix(model))
                    .AppendLine("));");
            }
            else
            {
                sb.Append("            global::Autofac.Extras.AggregateService.GeneratedAggregateServiceRegistry.Register(typeof(")
                    .Append(model.InterfaceFullyQualifiedName)
                    .Append("), static context => new global::")
                    .Append(QualifiedBackingClass(model))
                    .AppendLine("(context));");
            }
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void EmitMember(StringBuilder sb, string indent, MemberModel member)
    {
        switch (member.Kind)
        {
            case MemberKind.Property:
                EmitProperty(sb, indent, member);
                break;

            case MemberKind.ParameterlessMethod:
                EmitParameterlessMethod(sb, indent, member);
                break;

            case MemberKind.MethodWithParameters:
                EmitMethodWithParameters(sb, indent, member);
                break;

            case MemberKind.ThrowingVoidMethod:
                EmitThrowingVoidMethod(sb, indent, member);
                break;
        }
    }

    private static void EmitProperty(StringBuilder sb, string indent, MemberModel member)
    {
        sb.Append(indent)
            .Append(PublicModifier)
            .Append(member.ReturnType)
            .Append(' ')
            .Append(member.Name)
            .AppendLine();
        sb.Append(indent).AppendLine("{");
        sb.Append(indent).Append("    get => ").Append(BackingFieldName(member.Name)).AppendLine(";");

        if (member.HasSetter)
        {
            // Matches the runtime: property setters are not supported and throw on invocation.
            sb.Append(indent)
                .AppendLine("    set => throw new global::System.InvalidOperationException(\"Property setters are not supported on aggregate services.\");");
        }

        sb.Append(indent).AppendLine("}");
    }

    private static void EmitParameterlessMethod(StringBuilder sb, string indent, MemberModel member)
    {
        var genericSuffix = BuildMethodTypeParameterList(member.TypeParameters);
        sb.Append(indent)
            .Append(PublicModifier)
            .Append(member.ReturnType)
            .Append(' ')
            .Append(member.Name)
            .Append(genericSuffix)
            .Append("()")
            .Append(BuildConstraintClauses(member.TypeParameters))
            .AppendLine();
        sb.Append(indent).AppendLine("{");
        sb.Append(indent)
            .Append("    return (")
            .Append(member.ReturnType)
            .Append(")global::Autofac.ResolutionExtensions.Resolve(")
            .Append(ContextField)
            .Append(", typeof(")
            .Append(member.ReturnType)
            .AppendLine("));");
        sb.Append(indent).AppendLine("}");
    }

    private static void EmitMethodWithParameters(StringBuilder sb, string indent, MemberModel member)
    {
        var genericSuffix = BuildMethodTypeParameterList(member.TypeParameters);
        sb.Append(indent)
            .Append(PublicModifier)
            .Append(member.ReturnType)
            .Append(' ')
            .Append(member.Name)
            .Append(genericSuffix)
            .Append('(')
            .Append(BuildParameterDeclarations(member.Parameters))
            .Append(')')
            .Append(BuildConstraintClauses(member.TypeParameters))
            .AppendLine();
        sb.Append(indent).AppendLine("{");

        // Build the TypedParameter array forwarding each argument, mirroring the runtime.
        sb.Append(indent).AppendLine("    var __autofacParameters = new global::Autofac.Core.Parameter[]");
        sb.Append(indent).AppendLine("    {");
        foreach (var parameter in member.Parameters)
        {
            sb.Append(indent)
                .Append("        new global::Autofac.TypedParameter(typeof(")
                .Append(parameter.Type)
                .Append("), ")
                .Append(parameter.Name)
                .AppendLine("),");
        }

        sb.Append(indent).AppendLine("    };");
        sb.Append(indent)
            .Append("    return (")
            .Append(member.ReturnType)
            .Append(")global::Autofac.ResolutionExtensions.Resolve(")
            .Append(ContextField)
            .Append(", typeof(")
            .Append(member.ReturnType)
            .AppendLine("), __autofacParameters);");
        sb.Append(indent).AppendLine("}");
    }

    private static void EmitThrowingVoidMethod(StringBuilder sb, string indent, MemberModel member)
    {
        var genericSuffix = BuildMethodTypeParameterList(member.TypeParameters);
        sb.Append(indent)
            .Append("public void ")
            .Append(member.Name)
            .Append(genericSuffix)
            .Append('(')
            .Append(BuildParameterDeclarations(member.Parameters))
            .Append(')')
            .Append(BuildConstraintClauses(member.TypeParameters))
            .AppendLine();
        sb.Append(indent).AppendLine("{");
        sb.Append(indent)
            .AppendLine("    throw new global::System.InvalidOperationException(\"Methods with a void return type are not supported on aggregate services.\");");
        sb.Append(indent).AppendLine("}");
    }

    private static void AppendHeader(StringBuilder sb)
    {
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable disable");
        sb.AppendLine("#pragma warning disable");
        sb.AppendLine();
    }

    // Uses the property name verbatim (property names are unique within an interface) so the
    // backing field name cannot collide via case-folding - e.g. distinct properties "Value" and
    // "value" would otherwise map to the same field. The "__" prefix / "_value" suffix keep it
    // clear of the property identifiers themselves.
    private static string BackingFieldName(string propertyName)
        => "__" + propertyName + "_value";

    private static string BuildTypeParameterList(EquatableArray<TypeParameterModel> typeParameters)
    {
        if (typeParameters.Count == 0)
        {
            return string.Empty;
        }

        var names = new string[typeParameters.Count];
        for (var i = 0; i < typeParameters.Count; i++)
        {
            names[i] = typeParameters[i].Name;
        }

        return "<" + string.Join(", ", names) + ">";
    }

    private static string BuildMethodTypeParameterList(EquatableArray<TypeParameterModel> typeParameters)
        => BuildTypeParameterList(typeParameters);

    // Builds the trailing constraint clauses (" where T : ... where U : ...") for a type
    // parameter list, or an empty string if none are constrained.
    private static string BuildConstraintClauses(EquatableArray<TypeParameterModel> typeParameters)
    {
        var sb = new StringBuilder();
        foreach (var typeParameter in typeParameters)
        {
            if (!string.IsNullOrEmpty(typeParameter.Constraints))
            {
                sb.Append(" where ").Append(typeParameter.Name).Append(" : ").Append(typeParameter.Constraints);
            }
        }

        return sb.ToString();
    }

    private static string BuildParameterDeclarations(EquatableArray<ParameterModel> parameters)
    {
        var parts = new string[parameters.Count];
        for (var i = 0; i < parameters.Count; i++)
        {
            parts[i] = parameters[i].Modifier + parameters[i].Type + " " + parameters[i].Name;
        }

        return string.Join(", ", parts);
    }

    private static string BuildInterfaceReference(AggregateServiceModel model)
    {
        // For open generics, implement the interface closed over the backing class's own type
        // parameters (which share the interface's parameter names).
        if (model.IsOpenGeneric)
        {
            return model.InterfaceFullyQualifiedName + BuildTypeParameterList(model.TypeParameters);
        }

        return model.InterfaceFullyQualifiedName;
    }

    private static string QualifiedBackingClass(AggregateServiceModel model)
    {
        var ns = string.IsNullOrEmpty(model.InterfaceNamespace)
            ? string.Empty
            : model.InterfaceNamespace + ".";
        return ns + model.BackingClassName;
    }

    private static string OpenTypeofExpression(AggregateServiceModel model)
    {
        // typeof(IThing<>) form for an open generic definition.
        var commas = new string(',', model.TypeParameters.Count - 1);
        return model.InterfaceFullyQualifiedName + "<" + commas + ">";
    }

    private static string OpenBackingTypeofSuffix(AggregateServiceModel model)
    {
        var commas = new string(',', model.TypeParameters.Count - 1);
        return "<" + commas + ">";
    }
}
