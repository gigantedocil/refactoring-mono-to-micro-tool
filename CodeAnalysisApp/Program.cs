using CodeAnalysisApp.Analyzer;
using System.Threading.Tasks;

namespace CodeAnalysisApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			await new RefactoringsAnalyzer().BeginAnalysis();
		}
	}
}
