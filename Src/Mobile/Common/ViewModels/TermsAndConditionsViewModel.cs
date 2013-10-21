using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Booking.Mobile.Client;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using Cirrious.MvvmCross.ExtensionMethods;
using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.Text;
using apcurium.Framework.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class TermsAndConditionsViewModel : BaseSubViewModel<bool>,                 
        IMvxServiceConsumer<ITermsAndConditionsService>

	{

        public TermsAndConditionsViewModel ( string messageId ) : base(messageId)
		{

		}

		public IMvxCommand RejectTermsAndConditions
		{
			get
			{
                return GetCommand(() => { 
                    ReturnResult(false);
                    Console.WriteLine("Rejected T&C");
				});
			}
		}

		public IMvxCommand AcceptTermsAndConditions
		{
			get
			{
				return GetCommand (() =>
				{
                    ReturnResult(true);
                    Console.WriteLine("Accept T&C");
				});

			}			
		}

        private string _text;
        public string TermsAndConditions { 
            get 
            { 
                if (_text.IsNullOrEmpty())
                {
                    var service = this.GetService<ITermsAndConditionsService>();
                    _text = service.GetText();
                }
                return @_text; 
            } 
        }
	}
}

