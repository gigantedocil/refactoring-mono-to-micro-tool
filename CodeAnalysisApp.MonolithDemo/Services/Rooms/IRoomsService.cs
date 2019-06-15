using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonolithDemo.Services.Rooms
{
	public interface IRoomsService
	{
		bool RoomExists(int roomId);
		float GetRoomPricing(int roomId);
	}
}
