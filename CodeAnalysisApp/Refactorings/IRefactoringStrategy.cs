using Microsoft.CodeAnalysis;

namespace CodeAnalysisApp.Refactorings
{
	public interface IRefactoringStrategy
	{
		void ApplyRefactoring(Solution workspace);
	}
}
