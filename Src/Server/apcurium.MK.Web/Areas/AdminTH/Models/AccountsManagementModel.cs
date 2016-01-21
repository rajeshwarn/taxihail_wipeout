using apcurium.MK.Booking.ReadModel;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
	public class AccountsManagementModel
	{
	    [Display(Name = "Search Criteria")]
		public string SearchCriteria { get; set; }

		public AccountDetail[] Accounts { get; set; }

		public string[] CountryDialCode { get; set; }
	}
}