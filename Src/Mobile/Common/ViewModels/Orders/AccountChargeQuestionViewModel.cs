using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using ServiceStack.Text;
using Cirrious.MvvmCross.Plugins.PhoneCall;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class AccountChargeQuestionViewModel
	{
		public AccountChargeQuestionViewModel (AccountChargeQuestion model)
		{
			Model = model;
		}

		public AccountChargeQuestion Model {
			get;
			private set;
		}

		public string QuestionLabel
		{
			get
			{
				return string.Format ("{0} {1}",
					Model.Question,
					Model.IsRequired ? "required" : string.Empty);
			}
		}

		public string QuestionPlaceholder
		{
			get{
				return Model.MaxLength.HasValue ? 
					string.Format ("{0} characters maximum", Model.MaxLength)
						: string.Empty;
			}
		}
	}
}

