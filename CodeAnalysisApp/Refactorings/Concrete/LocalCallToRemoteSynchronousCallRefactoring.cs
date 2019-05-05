using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisApp.Refactorings.Concrete
{
	public class LocalCallToRemoteSynchronousCallRefactoring : IRefactoringStrategy
	{
		public LocalCallToRemoteSynchronousCallRefactoring() { }

		public async Task ApplyRefactoring(Solution solution)
		{
			// Debug
			var project = solution.Projects.Where(p => p.Name == "MonolithDemo").FirstOrDefault();

			var document = project.Documents.Where(d => d.Name == "RoomsService.cs").FirstOrDefault();

			if (document.SupportsSyntaxTree)
			{
				var syntaxTree = await document.GetSyntaxTreeAsync();
				var treeRoot = await syntaxTree.GetRootAsync();

				var a = treeRoot.ChildNodes();			

				var b = treeRoot.DescendantNodes();

				Console.WriteLine(syntaxTree);
			}

			if (document.SupportsSemanticModel)
			{
				var semanticModel = await document.GetSemanticModelAsync();
				Console.WriteLine(semanticModel);
			}									

			throw new NotImplementedException();
		}
	}
}
