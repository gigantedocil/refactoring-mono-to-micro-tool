using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeAnalysisApp.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisApp.Refactorings.Concrete
{
	public class LocalCallToRemoteSynchronousCallRefactoring : IRefactoringStrategy
	{
		private ISet<Document> SolutionDocuments = new HashSet<Document>();

		private ISet<DocumentAnalyzerAggregate> documentsRegistry = new HashSet<DocumentAnalyzerAggregate>();

		private readonly string ProjectName = "MonolithDemo";

		private readonly string ClassName = "RoomsService.cs";

		private readonly string MethodName = "CalculatePrice";

		public async Task ApplyRefactoring(Solution solution)
		{			
			await InitializeDocumentRegistry(solution);
			
			await GetInvokationMethodType(solution);			
		}

		private void GetSolutionClasses(Solution solution)
		{
			foreach (var project in solution.Projects)
			{
				foreach (var document in project.Documents)
				{
					SolutionDocuments.Add(document);
				}
			}
		}

		private async Task GetInvokationMethodType(Solution solution)
		{
			var project = solution.Projects.Where(p => p.Name == ProjectName).FirstOrDefault();

			var document = project.Documents.Where(d => d.Name == ClassName).FirstOrDefault();

			var syntaxTree = await document.GetSyntaxTreeAsync();

			var methodInvocations = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();

			var method = methodInvocations.FirstOrDefault(x => x.ToString().Contains(MethodName));

			var semanticModel = await document.GetSemanticModelAsync();

			var invokedMethodMetadata = semanticModel.GetSymbolInfo(method).Symbol;

			var typeFullName = invokedMethodMetadata.ContainingType.ToDisplayString();
		
			var selected = documentsRegistry.FirstOrDefault(x => x.DocumentTypeFullName == typeFullName);

			// TODO: Get syntax tree of type so I can get all classes of that file and do that recursively.

			var treeRoot = await syntaxTree.GetRootAsync();

			var classDeclaration = treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList().FirstOrDefault();

			var list6 = new HashSet<IdentifierNameSyntax>(classDeclaration.DescendantNodes().OfType<IdentifierNameSyntax>().ToList());
			var list7 = treeRoot.DescendantNodes().OfType<TypeSyntax>().ToList();

			// If it is an interface get implementations.
			// Otherwise search for types inside types.
		}

		public async Task InitializeDocumentRegistry(Solution solution)
		{
			foreach (var project in solution.Projects)
			{
				foreach (var document in project.Documents)
				{
					documentsRegistry.Add(
						new DocumentAnalyzerAggregate()
						{
							Document = document,
							SyntaxTree = await document.GetSyntaxTreeAsync(),
							SemanticModel = await document.GetSemanticModelAsync(),
							DocumentTypeFullName = await GetNameFromDocument(document)
						}
					);
				}
			}

		}

		public async Task<string> GetNameFromDocument(Document document)
		{
			var syntaxTree = await document.GetSyntaxTreeAsync();
			var semanticModel = await document.GetSemanticModelAsync();
			var root = syntaxTree.GetRoot();

			INamedTypeSymbol typeInfo = null;

			MemberAccessExpressionSyntax member = GetMemberAccessExpressionSyntax(root);
			if (member != null)
			{
				var firstChild = member.ChildNodes().ElementAt(0);
				typeInfo = semanticModel.GetTypeInfo(firstChild).Type as INamedTypeSymbol;
			}

			return typeInfo.ToDisplayString();
		}

		public MemberAccessExpressionSyntax GetMemberAccessExpressionSyntax(SyntaxNode node)
		{
			return node.DescendantNodes().Where(curr => curr is MemberAccessExpressionSyntax)
				.ToList().FirstOrDefault() as MemberAccessExpressionSyntax;
		}
	}
}
