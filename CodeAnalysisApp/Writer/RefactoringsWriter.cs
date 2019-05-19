using System.Collections.Generic;
using System.IO;

namespace CodeAnalysisApp.Writer
{
	public class RefactoringsWriter
	{
		private readonly string microserviceApplicationUrl = "pricingServiceApplicationUrl";
		private readonly string microserviceConfigurationKey = "PricingMicroservice";
		private readonly string methodName = "";
		private readonly string wrapperMethodName = "GetRoomPricing";
		private readonly string wrapperMethodSignature = "public float GetRoomPricing(int roomId)";
		private readonly string fileReadLocation = @"C:\Users\Me\Desktop\RoomsService.cs";
		private readonly string fileWriteLocation = @"C:\Users\Me\Desktop\RoomsDemoService.cs";

		public void BeginWriting()
		{
			var lines = new List<string>(File.ReadAllLines(fileReadLocation));

			WriteUsings(lines);
			WriteFields(lines);
			WriteConstructor(lines);
			WriteMethodSignature(lines);
			WriteMethod(lines);

			var file = string.Join("\n", lines);

			File.WriteAllText(fileWriteLocation, file);
		}

		private void WriteUsings(List<string> lines)
		{
			if (lines.FindIndex(l => l.Contains("using System.Threading.Tasks;")) == -1)
			{
				lines.Insert(0, "using System.Threading.Tasks;");
			}

			if (lines.FindIndex(l => l.Contains("using Newtonsoft.Json;")) == -1)
			{
				lines.Insert(0, "using Newtonsoft.Json;");
			}
		}

		private void WriteFields(List<string> lines)
		{
			var index = lines.FindIndex(l => l.Contains("public RoomsService"));

			lines.Insert(index, "");

			lines.Insert(index, "\t\tprivate readonly HttpClient httpClient;");
			lines.Insert(index, $"\t\tprivate readonly string {microserviceApplicationUrl};");
		}

		private void WriteConstructor(List<string> lines)
		{
			// TODO: Consider inline case.
			int index = lines.FindIndex(l => l.Contains("public RoomsService"));

			// TODO: What if the import already exists?

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
		}

		private void WriteMethodSignature(List<string> lines)
		{			
			var index = lines.FindIndex(l => l.Contains(wrapperMethodSignature));
			var aux = new List<string>(lines[index].Split(' '));
			var innerIndex = aux.FindIndex(x => x.Contains(wrapperMethodName));

			aux[innerIndex - 1] = $"Task<{aux[innerIndex - 1]}>";
			lines[index] = string.Join(" ", aux);
		}		

		private void WriteMethod(List<string> lines)
		{
			int index = lines.FindIndex(l => l.Contains("var roomPrice =  pricingService.CalculatePrice(roomType);"));

			lines.Insert(index, "\t\t\tvar roomPrice = await response.Content.ReadAsAsync<float>();");
			lines.Insert(index, "\t\t\tvar response = await httpClient.PostAsync(url, new StringContent(body, Encoding.UTF8, \"application/json\"));");
			lines.Insert(index, "\t\t\tvar body = JsonConvert.SerializeObject(roomType);");
			lines.Insert(index, "\t\t\tvar url = pricingServiceApplicationUrl + \"Pricing/CalculatePrice\";");
			lines.RemoveAt(index + 4);
		}
	}
}