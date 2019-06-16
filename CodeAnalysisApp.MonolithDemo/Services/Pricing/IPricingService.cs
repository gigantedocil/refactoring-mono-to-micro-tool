using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonolithDemo.Services.Pricing
{
	public interface IPricingService
	{
		float CalculatePrice(string RoomType, string mock);
	}
}
