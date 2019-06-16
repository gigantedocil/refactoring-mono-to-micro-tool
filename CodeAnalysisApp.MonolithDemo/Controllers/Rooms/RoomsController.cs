using System;
using Microsoft.AspNetCore.Mvc;
using MonolithDemo.Services.Rooms;

namespace MonolithDemo.Controllers.Rooms
{
	[Route("api/[controller]")]
	[ApiController]
	public class RoomsController : ControllerBase
	{
		private readonly IRoomsService roomsService;

		public RoomsController(IRoomsService roomsService)
		{
			this.roomsService = roomsService;
		}

		[HttpGet("{roomId}/price")]
		public IActionResult GetRoomPrices([FromRoute] int roomId)
		{
			if (!roomsService.RoomExists(roomId))
			{
				return NotFound("The specified room ID does not exist");
			}

			try
			{
				var roomPrice = roomsService.GetRoomPricing(roomId);

				return Ok(roomPrice);
			}
			catch (Exception e)
			{
				return BadRequest(e);
			}
		}
	}
}