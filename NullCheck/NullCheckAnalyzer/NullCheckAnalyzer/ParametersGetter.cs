using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullCheckAnalyzer
{
    internal sealed class ParametersGetter
    {
        private static readonly ImmutableHashSet<string> nullableAttributes = new[]
{
            "NotNull", "CanBeNull"
        }.ToImmutableHashSet();

        public static ImmutableArray<IParameterSymbol> GetParametersToFix(IMethodSymbol method)
        {
            return method
                .Parameters
                .Where(p => !p.GetAttributes().Any(a => nullableAttributes.Contains(a.AttributeClass.Name)))
                .ToImmutableArray();
        }

        public static ImmutableArray<ParameterSyntax> GetParametersToFix(BaseMethodDeclarationSyntax method)
        {
            return method.ParameterList
                .Parameters
                .Where(p => !p.AttributeLists.SelectMany(a => a.Attributes).Any(a => nullableAttributes.Contains(a.Name.FullSpan.ToString())))
                .ToImmutableArray();
        }
    }
}