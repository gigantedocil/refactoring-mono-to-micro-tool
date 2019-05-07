using CodeAnalysisApp.Writer;
using System.Threading.Tasks;

namespace CodeAnalysisApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			// await new RefactoringsAnalyzer().BeginAnalysis();
			new RefactoringsWriter().BeginWriting();
		}
	}

	// TODO: Usar HTTP POST sempre a rota fica o nome da classe mais o nome do método.
}
