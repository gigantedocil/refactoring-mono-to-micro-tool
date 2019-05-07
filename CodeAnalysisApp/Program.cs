using CodeAnalysisApp.Analyzer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeAnalysisApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			WriteFile();
		}

		private static void WriteFile() {

			// Variables.
			string microserviceApplicationUrl = "pricingServiceApplicationUrl";
			string microserviceConfigurationKey = "PricingMicroservice";
			string methodName = "";
			string wrapperMethodName = "GetRoomPricing";
			string wrapperMethodSignature = "public float GetRoomPricing(int roomId)";

			var fileReadLocation = @"C:\Users\Findmore\Desktop\RoomsService.cs";
			var fileWriteLocation = @"C:\Users\Findmore\Desktop\RoomsDemoService.cs";

			// await new RefactoringsAnalyzer().BeginAnalysis();
			var lines = new List<string>(File.ReadAllLines(fileReadLocation));

			var index = lines.FindIndex(l => l.Contains("public RoomsService"));

			lines.Insert(index, "");

			lines.Insert(index, "\t\tprivate readonly HttpClient httpClient;");
			lines.Insert(index, $"\t\tprivate readonly string {microserviceApplicationUrl};");

			// Consider inline case.
			index = lines.FindIndex(l => l.Contains("public RoomsService"));

			// What if the import already exists?
			lines.Insert(index + 1, "\t\t\tIHttpClientFactory clientFactory,");
			lines.Insert(index + 1, "\t\t\tIConfiguration configuration,");

			// Inside constructor.
			for (int i = index; i < lines.Count; i++)
			{
				if (lines[i].Contains("{"))
				{
					lines.Insert(i + 1, "\t\t\thttpClient = clientFactory.CreateClient();");
					lines.Insert(i + 1, $"\t\t\tpricingServiceApplicationUrl = configuration[\"{microserviceConfigurationKey}:ApplicationUrl\"];");
					break;
				}
			}

			// Inside the method.
			index = lines.FindIndex(l => l.Contains("var roomPrice =  pricingService.CalculatePrice(roomType);"));

			lines.Insert(index, "\t\t\tvar roomPrice = await response.Content.ReadAsAsync<float>();");
			lines.Insert(index, "\t\t\tvar response = await httpClient.GetAsync(url);");
			lines.Insert(index, "\t\t\tvar url = pricingServiceApplicationUrl + \"Pricing/\" + \"CalculatePrice ? \" + nameof(roomType) + \" = \" + roomType;");
			lines.RemoveAt(index + 3);

			// TODO change method signature with Task
			if (lines.FindIndex(l => l.Contains("using System.Threading.Tasks;")) == -1)
			{
				lines.Insert(0, "using System.Threading.Tasks;");
			}
			
			index = lines.FindIndex(l => l.Contains(wrapperMethodSignature));
			var aux = new List<string>(lines[index].Split(' '));
			var innerIndex = aux.FindIndex(x => x.Contains(wrapperMethodName));

			aux[innerIndex - 1] = $"Task<{aux[innerIndex - 1]}>";
			lines[index] = string.Join(" ", aux);

			string text = string.Join("\n", lines);

			File.WriteAllText(fileWriteLocation, text);
		}
	}
}
