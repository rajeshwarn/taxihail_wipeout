using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookDetailViewModel : BaseViewModel
    {
        private string _order;
        public BookDetailViewModel(string order)
        {
            Order = order;
        }
        
        public string Order
        {
            get { return _order; }
            set
            {
                _order = value;
                FirePropertyChanged(() => Order);
            }
        }
    }
}