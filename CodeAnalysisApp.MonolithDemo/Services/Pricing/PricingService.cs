using System.Collections.Generic;

namespace MonolithDemo.Services.Pricing
{
	public class PricingService : IPricingService
	{
		private static readonly Dictionary<string, float> roomPricings = new Dictionary<string, float>()
		{
			{ "Normal", 12 },
			{ "Premium", 32 }
		};

		public float CalculatePrice(string roomType, string mock)
		{
            new MockyMock();

			return roomPricings.GetValueOrDefault(roomType);
		}
	}
}
