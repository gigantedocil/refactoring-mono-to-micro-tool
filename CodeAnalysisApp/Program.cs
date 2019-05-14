using CodeAnalysisApp.Analyzer;
using CodeAnalysisApp.Writer;
using System.Threading.Tasks;

namespace CodeAnalysisApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			await new RefactoringsAnalyzer().BeginAnalysis();
			new RefactoringsWriter().BeginWriting();
		}
	}

	// TODO: Usar HTTP POST sempre a rota fica o nome da classe mais o nome do método.
	// TODO: Ir buscar todas as classes da solution e depois quando se está a ir buscar cada um dos objetos 
	// usados na classe verificar se fazem parte dessa primeira lista. Hope this makes sense tomorrow :)
}
