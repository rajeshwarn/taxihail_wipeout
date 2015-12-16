using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
	public class MarketFeesModel
	{
		public MarketFeesModel()
		{
			Fees = new Dictionary<string, FeeStructure>();
		}

		public Dictionary<string, FeeStructure> Fees { get; set; }
	}

	public class FeeStructure
	{
		[Display(Name = "Booking")]
		[Range(0, double.MaxValue, ErrorMessage = "The value must be a positive decimal number")]
		public decimal Booking { get; set; }

		[Display(Name = "Cancellation")]
		[Range(0, double.MaxValue, ErrorMessage = "The value must be a positive decimal number")]
		public decimal Cancellation { get; set; }

		[Display(Name = "No Show")]
		[Range(0, double.MaxValue, ErrorMessage = "The value must be a positive decimal number")]
		public decimal NoShow { get; set; }
	}
}