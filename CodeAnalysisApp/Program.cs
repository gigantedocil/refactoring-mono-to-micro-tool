using CodeAnalysisApp.Analyzer;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CodeAnalysisApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var fileReadLocation = @"C:\Users\Findmore\Desktop\RoomsService.cs";
			var fileWriteLocation = @"C:\Users\Findmore\Desktop\RoomsDemoService.cs";

			// await new RefactoringsAnalyzer().BeginAnalysis();
			var lines = new List<string>(File.ReadAllLines(fileReadLocation));

			var index = lines.FindIndex(l => l.Contains("public RoomsService"));			

			lines.Insert(index, "");

			lines.Insert(index, "\t\tprivate readonly HttpClient httpClient;");

			// Consider inline case.
			index = lines.FindIndex(l => l.Contains("public RoomsService"));

			lines.Insert(index + 1, "\t\t\tIHttpClientFactory clientFactory,");

			for (int i = index; i < lines.Count; i++)
			{
				if (lines[i].Contains("{"))
				{
					lines.Insert(i + 1, "\t\t\thttpClient = clientFactory.CreateClient();");
					break;
				}
			}

			string text = string.Join("\n", lines);			
			
			File.WriteAllText(fileWriteLocation, text);
		}
	}
}
