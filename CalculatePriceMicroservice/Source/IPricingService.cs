using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalculatePriceMicroservice.Source
{
	public interface IPricingService
	{
		float CalculatePrice(string RoomType, string mock);
	}
}