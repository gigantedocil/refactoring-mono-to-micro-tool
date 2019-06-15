namespace MonolithDemo.Repositories.Rooms
{
	public interface IRoomsRepository
	{
		bool RoomExists(int roomId);
		string GetRoomsType(int roomId);
	}
}
