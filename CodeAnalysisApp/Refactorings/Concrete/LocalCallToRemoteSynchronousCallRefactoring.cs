﻿using System.Collections.Generic;
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

		private readonly string ProjectName = "MonolithDemo";

		private readonly string ClassName = "RoomsService.cs";

		private readonly string MethodName = "CalculatePrice";

		public async Task ApplyRefactoring(Solution solution)
		{
			GetSolutionClasses(solution);

			var project = solution.Projects.Where(p => p.Name == ProjectName).FirstOrDefault();

			var document = project.Documents.Where(d => d.Name == ClassName).FirstOrDefault();

			var semanticModel = await document.GetSemanticModelAsync();

			var syntaxTree = await document.GetSyntaxTreeAsync();

			var invocationSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();
			var invokedSymbol = semanticModel.GetSymbolInfo(invocationSyntax).Symbol;

			await GetInvokationMethodType(solution);

			if (document.SupportsSyntaxTree)
			{

				var treeRoot = await syntaxTree.GetRootAsync();

				var list1 = treeRoot.DescendantNodes().OfType<SimpleNameSyntax>().ToList();
				var list2 = treeRoot.DescendantNodes().OfType<MemberDeclarationSyntax>().ToList();
				var list3 = treeRoot.DescendantNodes().OfType<CompilationUnitSyntax>().ToList();
				var classDeclaration = treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList().FirstOrDefault();

				var list5 = treeRoot.DescendantNodes().OfType<ClassOrStructConstraintSyntax>().ToList();
				var list6 = new HashSet<IdentifierNameSyntax>(classDeclaration.DescendantNodes().OfType<IdentifierNameSyntax>().ToList());
				var list7 = treeRoot.DescendantNodes().OfType<TypeSyntax>().ToList();

				var a = treeRoot.ChildNodes();

				var b = treeRoot.DescendantNodes();
			}
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

			var type = invokedMethodMetadata.ContainingType;

			var documents = project.Documents;

			var documentsTrees = new HashSet<SyntaxTree>();

			var semanticModels = new HashSet<SemanticModel>();

			var documentAnalyzerAggregates = new HashSet<DocumentAnalyzerAggregate>();

			foreach (var documenta in documents)
			{
				documentAnalyzerAggregates.Add(
					new DocumentAnalyzerAggregate()
					{
						Document = documenta,
						SyntaxTree = await documenta.GetSyntaxTreeAsync(),
						SemanticModel = await documenta.GetSemanticModelAsync(),
						DocumentTypeFullName = await GetNameFromDocument(documenta)
					}
				);				
			}

			// Won't work if there are multiple files with the same name.
			var selected = documentsTrees.FirstOrDefault(x => x.FilePath.Contains(type.ToDisplayString()));

			// TODO: Get syntax tree of type so I can get all classes of that file and do that recursively.

			var treeRoot = await syntaxTree.GetRootAsync();

			var classDeclaration = treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList().FirstOrDefault();

			var list6 = new HashSet<IdentifierNameSyntax>(classDeclaration.DescendantNodes().OfType<IdentifierNameSyntax>().ToList());
			var list7 = treeRoot.DescendantNodes().OfType<TypeSyntax>().ToList();

			// If it is an interface get implementations.
			// Otherwise search for types inside types.
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
