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
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookAddressViewModel
    {
        public BookAddressViewModel( Address address )
        {
            Model = address;
        }
        public string Title { get; set; }
        public string EmptyAddressPlaceholder { get; set; }

        public string Display
        {
            get
            {
                return EmptyAddressPlaceholder;
            }
        }
            

        public Address Model { get; private set; }
    }
}