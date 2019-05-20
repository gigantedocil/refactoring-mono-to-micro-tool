using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace CodeAnalysisApp.Refactorings
{
    public interface IRefactoringStrategy
    {
        Task ApplyRefactoring(Solution solution);
    }
}
