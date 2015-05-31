using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NullCheckAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NullCheckAnalyzer : DiagnosticAnalyzer
    {
        public const string ParameterIsNullId = "NullCheckAnalyzer_MethodContainNulls";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(ParameterIsNullId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var methodSymbol = context.Symbol as IMethodSymbol;

            if (ReferenceEquals(null, methodSymbol) || methodSymbol.DeclaredAccessibility == Accessibility.Private)
            {
                return;
            }

            foreach (var parameter in ParametersGetter.GetParametersToFix(methodSymbol))
            {
                var type = methodSymbol.ContainingType;

                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], methodSymbol.Name, type.Name, parameter.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}