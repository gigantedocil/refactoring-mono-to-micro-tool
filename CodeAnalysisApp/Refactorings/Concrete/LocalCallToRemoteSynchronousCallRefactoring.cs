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
			// Debug
			var project = solution.Projects.Where(p => p.Name == "MonolithDemo").FirstOrDefault();



			//var compilation = await project.GetCompilationAsync();

			//var classVisitor = new ClassVirtualizationVisitor();
			//classVisitor.Visit(semanticModel.SyntaxTree.getRoot());

			//var classes = classVisitor.classes;

			var document = project.Documents.Where(d => d.Name == "RoomsService.cs").FirstOrDefault();

			var semanticModel = document.GetSemanticModelAsync();

			//var classVisitor = new ClassVirtualizationVisitor();
			//classVisitor.Visit(semanticModel.);

			if (document.SupportsSyntaxTree)
			{
				var syntaxTree = await document.GetSyntaxTreeAsync();
				var treeRoot = await syntaxTree.GetRootAsync();

				var list1 = treeRoot.DescendantNodes().OfType<SimpleNameSyntax>().ToList();
				var list2 = treeRoot.DescendantNodes().OfType<MemberDeclarationSyntax>().ToList();
				var list3 = treeRoot.DescendantNodes().OfType<CompilationUnitSyntax>().ToList();
				var classDeclaration = treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList().FirstOrDefault();

				var list5 = treeRoot.DescendantNodes().OfType<ClassOrStructConstraintSyntax>().ToList();
				var list6 = new HashSet<IdentifierNameSyntax>(classDeclaration.DescendantNodes().OfType<IdentifierNameSyntax>().ToList());
				var list7 = treeRoot.DescendantNodes().OfType<TypeSyntax>().ToList();

				var classVisitor = new ClassVirtualizationVisitor();
				//classVisitor.Visit(treeRoot);

				//var classes = classVisitor.classes; // li

				//foreach (UsingDirectiveSyntax element in root.Usings)
				//{
				//	var bc = element;
				//}
				//Classifier.GetClassifiedSpans()

				var a = treeRoot.ChildNodes();

				var b = treeRoot.DescendantNodes();

				//Console.WriteLine(syntaxTree);
			}

			//if (document.SupportsSemanticModel)
			//{
			//	var semanticModel = await document.GetSemanticModelAsync();
			//	Console.WriteLine(semanticModel);
			//}

			//throw new NotImplementedException();
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
