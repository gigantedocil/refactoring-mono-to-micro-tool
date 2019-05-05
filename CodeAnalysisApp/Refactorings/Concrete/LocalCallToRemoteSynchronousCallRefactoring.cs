using System;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisApp.Refactorings.Concrete
{
	public class LocalCallToRemoteSynchronousCallRefactoring : IRefactoringStrategy
	{
		public LocalCallToRemoteSynchronousCallRefactoring() { }

		public void ApplyRefactoring(Solution workspace)
		{			
			// Debug

			throw new NotImplementedException();
		}
	}
}
