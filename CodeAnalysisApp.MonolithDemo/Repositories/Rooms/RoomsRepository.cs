using System.Collections.Generic;

namespace MonolithDemo.Repositories.Rooms
{
	public class RoomsRepository : IRoomsRepository
	{
		private readonly static Dictionary<int, string> rooms = new Dictionary<int, string>()
		{
			{1, "Normal" },
			{2, "Premium" }
		};

		public string GetRoomsType(int roomId)
		{
			return rooms.GetValueOrDefault(roomId);
		}

		public bool RoomExists(int roomId)
		{
			var roomExists = rooms.ContainsKey(roomId);

			return roomExists;
		}
	}
}
