using MonolithDemo.Repositories.Rooms;
using MonolithDemo.Services.Pricing;

namespace MonolithDemo.Services.Rooms
{
	public class RoomsService : IRoomsService
	{
		private readonly IPricingService pricingService;
		private readonly IRoomsRepository roomsRepository;

		public RoomsService(
			IPricingService pricingService,
			IRoomsRepository roomsRepository)
		{
			this.pricingService = pricingService;
			this.roomsRepository = roomsRepository;
		}

		public float GetRoomPricing(int roomId)
		{
			var roomType = roomsRepository.GetRoomsType(roomId);

			var roomPrice =  pricingService.CalculatePrice(roomType, "");

			return roomPrice;
		}

		public bool RoomExists(int roomId)
		{
			return roomsRepository.RoomExists(roomId);
		}
	}
}
