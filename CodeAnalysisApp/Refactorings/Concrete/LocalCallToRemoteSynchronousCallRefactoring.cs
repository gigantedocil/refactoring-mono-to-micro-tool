using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeAnalysisApp.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

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

			var semanticModel = await document.GetSemanticModelAsync();

			var syntaxTree = await document.GetSyntaxTreeAsync();

			var methodInvocations = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();

			var invokedMethod = methodInvocations.FirstOrDefault(x => x.ToString().Contains(MethodName));

			var invokedMethodMetadata = semanticModel.GetSymbolInfo(invokedMethod).Symbol;

			var typeFullName = invokedMethodMetadata.ContainingType.ToDisplayString();

			var invokedMethodDocument = documentsRegistry.FirstOrDefault(x => x.DocumentTypeFullName == typeFullName);

			await RecursiveMethod(invokedMethodDocument, solution);
		}

		private async Task RecursiveMethod(DocumentAnalyzerAggregate document, Solution solution)
		{
			var treeRoot = await document.SyntaxTree.GetRootAsync();

			// TODO: Get syntax tree of type so I can get all classes of that file and do that recursively.

			// If is class declaration buscar todas as classes if interface buscar interface implementations e 
			// We're assuming each file only has a class
			var isClass = treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault() != null;

			var interfaceest = treeRoot.DescendantNodes().OfType<InterfaceDeclarationSyntax>().FirstOrDefault();

			if (isClass)
			{

			}

			if (interfaceest != null)
			{

				var interfaceSymbol = document.SemanticModel.GetSymbolInfo(interfaceest).Symbol;

				var implementations = await SymbolFinder.FindImplementationsAsync(interfaceSymbol, solution);
			}

			var classDeclaration = treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList().FirstOrDefault();

			var list6 = new HashSet<IdentifierNameSyntax>(classDeclaration.DescendantNodes().OfType<IdentifierNameSyntax>().ToList());
			var list7 = treeRoot.DescendantNodes().OfType<TypeSyntax>().ToList();

			// If it is an interface get implementations.
			// Otherwise search for types inside types.
		}

		private async Task InitializeDocumentRegistry(Solution solution)
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

		private async Task<string> GetNameFromDocument(Document document)
		{
			var syntaxTree = await document.GetSyntaxTreeAsync();

			var root = syntaxTree.GetRoot();

			string typeName = null;

			string nameSpace = null;

			var namespaceDeclaration = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

			if (namespaceDeclaration != null)
			{
				nameSpace = namespaceDeclaration.Name.ToString();
			}

			var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

			if (classDeclaration == null)
			{
				var interfaceDeclaration = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().FirstOrDefault();

				if (interfaceDeclaration != null)
				{
					typeName = interfaceDeclaration.Identifier.ToString();
				}
			}
			else
			{
				typeName = classDeclaration.Identifier.ToString();
			}

			if (nameSpace == null || typeName == null)
			{
				return "";
			}

			return nameSpace + "." + typeName;
		}
	}
}
