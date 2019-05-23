using MicroservicesDemo.Sync.PricingMicroservice.Services.Pricing;
using Microsoft.AspNetCore.Mvc;

namespace MicroservicesDemo.Sync.PricingMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService pricingService;

        public PricingController(
            IPricingService pricingService)
        {
            this.pricingService = pricingService;
        }

        [HttpPost("{MethodName}")]
        public IActionResult {MethodName}([FromQuery] string roomType)
        {
            var result = pricingService.CalculatePrice(roomType);
            return Ok(result);
        }
    }
}