using CalculatePriceMicroservice.Source;
using Microsoft.AspNetCore.Mvc;

namespace CalculatePriceMicroservice.Controllers
{    
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService ipricingservice;

        public PricingController(
            IPricingService ipricingservice)
        {
            this.ipricingservice = ipricingservice;
        }

        [HttpPost("CalculatePrice")]
        public IActionResult CalculatePrice([FromBody] RequestData requestData)
        {
            var result = ipricingservice.CalculatePrice(requestData.RoomType, requestData.mock);
            return Ok(result);
        }
    }

	public class RequestData
	{
		public string RoomType { get; set; }
		public string mock { get; set; }
	}
}