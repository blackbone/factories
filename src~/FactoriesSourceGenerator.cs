using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class FactoriesSourceGenerator : ISourceGenerator
{
    private bool IsUnity { get; set; }
    private bool IsUnityEditor { get; set; }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver) return;
        
        IsUnity = context.ParseOptions.PreprocessorSymbolNames.Any(define => define.Contains("UNITY"));
        IsUnityEditor = context.ParseOptions.PreprocessorSymbolNames.Contains("UNITY_EDITOR");
        
        // not working outside unity
        if (!IsUnity) return;
        
        // if attribute type not found - skip
        var attributeType = context.Compilation.GetTypeByMetadataName("Factories.FactoryAttribute");
        if (attributeType == null) return;
        
        // check all candidate classes
        foreach (var candidateType in syntaxReceiver.Candidates)
        {
            // getting type symbol
            var classSemanticModel = context.Compilation.GetSemanticModel(candidateType.SyntaxTree);
            var typeSymbol = classSemanticModel.GetDeclaredSymbol(candidateType) as INamedTypeSymbol;
            if (typeSymbol == null) continue;
            
            // getting type attributes
            var attributes = typeSymbol.GetAttributes();
            if (attributes.Length == 0) continue;
            
            // check contains required attribute
            var factoryAttribute = attributes.FirstOrDefault(a => a.AttributeClass != null && a.AttributeClass.Equals(attributeType, SymbolEqualityComparer.Default));
            if (factoryAttribute == null) continue;
            
            // validate constructor arguments
            if (factoryAttribute.ConstructorArguments.Length == 0) continue;
            if (factoryAttribute.ConstructorArguments[0].Value == null) continue;
            if (factoryAttribute.ConstructorArguments[1].Value == null) continue;
            
            // extract constructor arguments values
            var type = factoryAttribute.ConstructorArguments[0].Value as INamedTypeSymbol;
            var keyType = factoryAttribute.ConstructorArguments[1].Type;
            var key = factoryAttribute.ConstructorArguments[1].Value;
            
            // check arguments are valid
            if (type == null) continue;
            if (keyType == null) continue;
            if (key == null) continue;
            
            // generate registration code
            var sb = new StringBuilder();
            sb.AppendLine("// THIS CODE IS AUTO GENERATED, YAY");
            sb.AppendLine("using Factories;");
            sb.AppendLine();
            sb.AppendLine($"namespace {typeSymbol.ContainingNamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"\tinternal static class {typeSymbol.Name}Registration");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\t[UnityEngine.RuntimeInitializeOnLoadMethod]");
            sb.AppendLine($"\t\tpublic static void Register() => Factory<{type}, {keyType}>.Register({Format(key, keyType)}, () => new {typeSymbol.Name}());");
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            context.AddSource($"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}.g.cs", sb.ToString());
        }
    }

    private static string Format(object key, ITypeSymbol type)
    {
        // special formatting rules for strings and enums
        if (type.SpecialType == SpecialType.System_String) return $"\"{key}\"";
        if (type.TypeKind == TypeKind.Enum) return $"({type}){key}";
        if (type.TypeKind is TypeKind.Struct or TypeKind.Class or TypeKind.Interface) return $"typeof({key})";
        return key.ToString();
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public readonly List<TypeDeclarationSyntax> Candidates = new();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // collecting candidate types
            if (syntaxNode is not TypeDeclarationSyntax typeDeclarationSyntax) return;
            var attributes = typeDeclarationSyntax.AttributeLists.SelectMany(al => al.Attributes).ToImmutableArray();
            if (attributes.Length == 0) return;
            if (attributes.All(a => !a.Name.ToString().Contains("Factory"))) return;
            Candidates.Add(typeDeclarationSyntax);
        }
    }
}
