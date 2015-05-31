using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullCheckAnalyzer
{
    internal static class ParametersGetter
    {
        private static readonly ImmutableHashSet<string> nullableAttributes = new[]
{
            "NotNull", "CanBeNull"
        }.ToImmutableHashSet();

        public static ImmutableArray<IParameterSymbol> GetParametersToFix(IMethodSymbol method)
        {
            return method
                .Parameters
                .Where(p => 
                    p.RefKind != RefKind.Out && 
                    p.Type.IsReferenceType && 
                    !p.GetAttributes().Any(a => nullableAttributes.Contains(a.AttributeClass.Name)))
                .ToImmutableArray();
        }
    }
}