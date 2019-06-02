using CodeAnalysisApp.Refactorings.Concrete;
using System.Collections.Generic;

namespace CodeAnalysisApp.Refactorings
{
	public class RefactoringStrategyFactory
	{
		private static readonly Dictionary<string, IRefactoringStrategy> refactorings = new Dictionary<string, IRefactoringStrategy> {
			{"localCallToRemoteSynchronousCall", new LocalCallToRemoteSynchronousCallRefactoring() }
		};

		public static IRefactoringStrategy Create(string refactoringName)
		{
			return refactorings[refactoringName];
		}
	}
}
