using System.Text;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using MonolithDemo.Repositories.Rooms;
using MonolithDemo.Services.Pricing;

namespace MonolithDemo.Services.Rooms
{
	public class RoomsService : IRoomsService
	{
		private readonly IPricingService pricingService;
		private readonly IRoomsRepository roomsRepository;

		private readonly string microserviceUrl;
		private readonly HttpClient httpClient;

		public RoomsService(
			IConfiguration configuration,
			IHttpClientFactory clientFactory,
			IPricingService pricingService,
			IRoomsRepository roomsRepository)
		{
			microserviceUrl = "http://localhost:48759/api";
			httpClient = clientFactory.CreateClient();
			this.pricingService = pricingService;
			this.roomsRepository = roomsRepository;
		}

		public float GetRoomPricing(int roomId)
		{
			var roomType = roomsRepository.GetRoomsType(roomId);

			var url = microserviceUrl + "/Pricing/CalculatePrice";
			var requestData = new RequestData() {
				RoomType = roomType,
				mock = ""
			};

			var body = JsonConvert.SerializeObject(requestData);
			var response = httpClient.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json")).Result;
			var roomPrice = response.Content.ReadAsAsync<float>().Result;

			return roomPrice;
		}

		public bool RoomExists(int roomId)
		{
			return roomsRepository.RoomExists(roomId);
		}
	}

	public class RequestData
	{
		public string RoomType { get; set; }
		public string mock { get; set; }
	}
}