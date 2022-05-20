using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DELTation.LeoEcsExtensions.CodeGen.Components.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using static DELTation.LeoEcsExtensions.CodeGen.Components.EcsComponentsExtensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DELTation.LeoEcsExtensions.CodeGen.Components.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CreateComponentViewFixProvider))] [Shared]
    public class CreateComponentViewFixProvider : CodeFixProvider
    {
        private const string Title = "Create a ComponentView";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(CreateComponentViewAnalyzer.CreateComponentViewId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root =
                await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                    .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var sds =
                root!.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf()
                    .OfType<StructDeclarationSyntax>().First();
            context.RegisterCodeFix(
                CodeAction.Create(Title, c =>
                        FixAsync(context.Document, sds, c), Title
                ), diagnostic
            );
        }

        private async Task<Document> FixAsync(Document document,
            StructDeclarationSyntax sds,
            CancellationToken cancellationToken)
        {
            SyntaxNode old;
            SyntaxNode @new;

            var fullIdentifier = sds.GetIdentifierWithTypeParameters();

            var componentViewSyntax = ClassDeclaration(sds.Identifier + "View")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBaseList(
                        BaseList(
                            SingletonSeparatedList<BaseTypeSyntax>(
                                SimpleBaseType(
                                    GenericName(
                                            Identifier("ComponentView")
                                        )
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList<TypeSyntax>(IdentifierName(fullIdentifier))
                                            )
                                        )
                                )
                            )
                        )
                    )
                ;
            if (sds.TypeParameterList != null)
                componentViewSyntax = componentViewSyntax.WithTypeParameterList(sds.TypeParameterList);
            componentViewSyntax = componentViewSyntax
                    .WithAdditionalAnnotations(Formatter.Annotation)
                ;

            string[] extraComponentAttributes =
            {
                EcsComponentWithView, Serializable,
            };

            if (SyntaxNodeHelper.TryGetParentSyntax(sds, out NamespaceDeclarationSyntax nds))
            {
                old = nds;
                @new = nds
                    .ReplaceNode(sds, sds.AddAttributes(extraComponentAttributes))
                    .AddMembers(componentViewSyntax);
            }
            else if (SyntaxNodeHelper.TryGetParentSyntax(sds, out CompilationUnitSyntax cus))
            {
                old = cus;
                @new = cus
                    .ReplaceNode(sds, sds.AddAttributes(extraComponentAttributes))
                    .AddMembers(componentViewSyntax);
            }
            else
            {
                return document;
            }

            var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
            root = root.ReplaceNode(old, @new);
            root = root
                    .TryAddUsingStatement("DELTation.LeoEcsExtensions.Views.Components")
                    .TryAddUsingStatement("DELTation.LeoEcsExtensions.Attributes")
                    .TryAddUsingStatement("System")
                ;
            var newDocument = document.WithSyntaxRoot(root);

            return newDocument;
        }
    }
}