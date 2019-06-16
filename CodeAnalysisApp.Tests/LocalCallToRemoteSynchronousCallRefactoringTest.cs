using System.Collections.Generic;
using System.IO;
using Xunit;

namespace CodeAnalysisApp.Tests
{
	public class LocalCallToRemoteSynchronousCallRefactoringTest
	{
		private string microservicePath;

		public LocalCallToRemoteSynchronousCallRefactoringTest()
		{
			var binPath = Directory.GetCurrentDirectory();
			var auxList = new List<string>(binPath.Split('\\'));

			auxList.RemoveAt(auxList.Count - 1);
			auxList.RemoveAt(auxList.Count - 1);
			auxList.RemoveAt(auxList.Count - 1);
			auxList.RemoveAt(auxList.Count - 1);

			var path = string.Join('\\', auxList);

			microservicePath = path + "\\CalculatePriceMicroservice\\";
		}

		[Fact]
		public void CheckIfProjectExists()
		{
			var filePath = microservicePath + "CalculatePriceMicroservice.csproj";

			Assert.True(File.Exists(filePath));
		}

		[Fact]
		public void CheckIfLaunchSettingsExists()
		{
			var filePath = microservicePath + "Properties\\launchSettings.json";

			Assert.True(File.Exists(filePath));
		}

		[Fact]
		public void CheckIfAppSettingsExists()
		{
			var filePath = microservicePath + "appsettings.json";

			Assert.True(File.Exists(filePath));
		}

		[Fact]
		public void CheckIfStartupExists()
		{
			var filePath = microservicePath + "Startup.cs";

			Assert.True(File.Exists(filePath));
		}

		[Fact]
		public void CheckIfProgramExists()
		{
			var filePath = microservicePath + "Program.cs";

			Assert.True(File.Exists(filePath));
		}

		[Fact]
		public void CheckIfPricingControllerExists()
		{
			var filePath = microservicePath + "Controllers\\PricingController.cs";

			Assert.True(File.Exists(filePath));
		}

		[Fact]
		public void CheckIfIPricingServiceExists()
		{
			var filePath = microservicePath + "Source\\IPricingService.cs";

			Assert.True(File.Exists(filePath));
		}

		[Fact]
		public void CheckIfMockyMockExists()
		{
			var filePath = microservicePath + "Source\\MockyMock.cs";

			Assert.True(File.Exists(filePath));
		}

		[Fact]
		public void CheckIfPricingService()
		{
			var filePath = microservicePath + "Source\\PricingService.cs";

			Assert.True(File.Exists(filePath));
		}
	}
}
