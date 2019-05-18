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
		private ISet<DocumentAnalyzerAggregate> documentsRegistry;

		private readonly string ProjectName = "MonolithDemo";

		private readonly string ClassName = "RoomsService.cs";

		private readonly string MethodName = "CalculatePrice";

		public async Task ApplyRefactoring(Solution solution)
		{
			documentsRegistry = await InitializeDocumentRegistry(solution);

			var invokedMethodDocument = await GetInvokationMethodType(solution);

			await RecursiveMethod(invokedMethodDocument, solution);
		}

		private async Task<HashSet<DocumentAnalyzerAggregate>> InitializeDocumentRegistry(Solution solution)
		{
			var documents = new HashSet<DocumentAnalyzerAggregate>();

			foreach (var project in solution.Projects)
			{
				foreach (var document in project.Documents)
				{
					documents.Add(
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

			return documents;
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

		private async Task<DocumentAnalyzerAggregate> GetInvokationMethodType(Solution solution)
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

			return invokedMethodDocument;
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
				var interfaceImplementations = await GetInterfaceImplementations(document);
			}
		}

		public async Task<HashSet<DocumentAnalyzerAggregate>> GetInterfaceImplementations(DocumentAnalyzerAggregate interfaceDocument)
		{
			var implementations = new HashSet<DocumentAnalyzerAggregate>();
			// document.SemanticModel.Compilation.GetTypeByMetadataName(document.DocumentTypeFullName);

			foreach (var document in documentsRegistry)
			{
				var root = await document.SyntaxTree.GetRootAsync();

				// We only care about classes implementing interfaces and not interfaces implementing interfaces.
				var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

				if (classDeclaration != null)
				{
					var name = interfaceDocument.Document.Name.Split('.')[0];

					var documentLines = classDeclaration.ToString().Split('\n');

					var classDeclarationLine = documentLines.FirstOrDefault(x => x.Contains(name));

					if (classDeclarationLine != null)
					{
						var classExtensions = classDeclarationLine.Split(':');

						if (classExtensions.Length > 1)
						{
							var classExtensionsList = classExtensions[1].Split(',');

							if (classExtensionsList.FirstOrDefault(x => x.Contains(name)) != null)
							{
								implementations.Add(document);
							}
						}
					}
				}
			}

			return implementations;
		}
	}
}
