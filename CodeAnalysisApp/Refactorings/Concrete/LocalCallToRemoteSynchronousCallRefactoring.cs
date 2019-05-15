using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisApp.Refactorings.Concrete
{
	public class LocalCallToRemoteSynchronousCallRefactoring : IRefactoringStrategy
	{
		public LocalCallToRemoteSynchronousCallRefactoring() { }

		public async Task ApplyRefactoring(Solution solution)
		{

			await GetSolutionClasses(solution);
			// Debug
			var project = solution.Projects.Where(p => p.Name == "MonolithDemo").FirstOrDefault();


			//solution.semanticModel
			//solution.semantic


			//var compilation = await project.GetCompilationAsync();

			//var classVisitor = new ClassVirtualizationVisitor();
			//classVisitor.Visit(semanticModel.SyntaxTree.getRoot());

			//var classes = classVisitor.classes;

			var document = project.Documents.Where(d => d.Name == "RoomsService.cs").FirstOrDefault();			

			var semanticModel = await document.GetSemanticModelAsync();

			var syntaxTree = await document.GetSyntaxTreeAsync();

			var invocationSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();
			var invokedSymbol = semanticModel.GetSymbolInfo(invocationSyntax).Symbol;			

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

		private async Task<int> GetSolutionClasses(Solution solution)
		{
			var nodes = new HashSet<InvocationExpressionSyntax>();
			var symbols = new HashSet<ISymbol>();
			var strings = new HashSet<string>();
			var documents = new HashSet<Document>();

			foreach (var project in solution.Projects)
			{
				foreach (var document in project.Documents)
				{
					documents.Add(document);

					var syntaxTree = await document.GetSyntaxTreeAsync();
					var treeRoot = await syntaxTree.GetRootAsync();

					foreach (var node in treeRoot.DescendantNodes().OfType<InvocationExpressionSyntax>())
					{
						nodes.Add(node);

						// strings.Add(node.Identifier.ValueText);

						var semanticModel = await document.GetSemanticModelAsync();
						var classDeclarationSymbol = semanticModel.GetSymbolInfo(node).Symbol;

						symbols.Add(classDeclarationSymbol);
					}
				}
			}

			return 0;
		}
	}

	public class ClassVirtualizationVisitor : CSharpSyntaxRewriter
	{
		public List<string> classes = new List<string>();

		public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

			string className = node.Identifier.ValueText;
			classes.Add(className); // save your visited classes

			return node;
		}
	}
}
