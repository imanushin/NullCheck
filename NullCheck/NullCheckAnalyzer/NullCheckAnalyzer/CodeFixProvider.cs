using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace NullCheckAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullCheckAnalyzerCodeFixProvider)), Shared]
    public class NullCheckAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(NullCheckAnalyzer.ParameterIsNullId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var token = root.FindToken(diagnosticSpan.Start);
            var parent = token.Parent;
            var elements = parent.AncestorsAndSelf().ToImmutableArray();
            var methods = elements.OfType<BaseMethodDeclarationSyntax>().ToImmutableArray();

            var declaration = methods.First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create("Make uppercase", c => AddAttributeForAllParameters(context.Document, declaration, c)),
                diagnostic);
        }

        private async Task<Document> AddAttributeForAllParameters(Document document, BaseMethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);

            var parametersToFix = ParametersGetter.GetParametersToFix(methodDeclaration);

            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var newRoot = root;

            foreach (var parameterSymbol in parametersToFix)
            {
                var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("NotNull"));
                var separateAttributeList = SyntaxFactory.SeparatedList(new[] { attribute });

                var newAttributeList = SyntaxFactory.AttributeList(separateAttributeList);

                var attributes = parameterSymbol.AttributeLists.Add(newAttributeList);
                var newParameter = SyntaxFactory.Parameter(
                    attributes,
                    parameterSymbol.Modifiers,
                    parameterSymbol.Type,
                    parameterSymbol.Identifier,
                    parameterSymbol.Default);

                newRoot = newRoot.ReplaceNode(parameterSymbol, newParameter);
            }

            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}