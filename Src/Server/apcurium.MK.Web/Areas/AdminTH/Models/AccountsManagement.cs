using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
	public class AccountsManagement
	{
		public AccountsManagement()
		{

		}

		[Display(Name = "Search Criteria")]
		public string SearchCriteria { get; set; }
	}
}