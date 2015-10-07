using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Commands
{
	public class CreateReportOrder:CreateOrder
	{
		public string Error { get; set; }

		public CreateReportOrder():base()
		{
		}
	}
}