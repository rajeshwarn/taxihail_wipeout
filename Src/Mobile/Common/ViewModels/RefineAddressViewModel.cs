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
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class RefineAddressViewModel : BaseViewModel
    {
        public RefineAddressViewModel(Address address)
        {
            
        }

        private string _streetNumber;
        public string StreetNumber
        {
            get { return _streetNumber; }
            set
            {
                _streetNumber = value;
                FirePropertyChanged(() => StreetNumber);
            }
        }

        private string _streetAddress;
        public string StreetAddress
        {
            get { return _streetAddress; }
            set
            {
                _streetAddress = value;
                FirePropertyChanged(() => StreetAddress);
            }
        }

        private string _aptNumber;
        public string AptNumber
        {
            get { return _aptNumber; }
            set
            {
                _aptNumber = value;
                FirePropertyChanged(() => AptNumber);
            }
        }

        private string _ringCode;
        public string RingCode
        {
            get { return _ringCode; }
            set
            {
                _ringCode = value;
                FirePropertyChanged(() => RingCode);
            }
        }

        private string _buildingName;
        public string BuildingName
        {
            get { return _buildingName; }
            set
            {
                _buildingName = value;
                FirePropertyChanged(() => BuildingName);
            }
        }

        public IMvxCommand SaveCommand
        {
            get
            {
                return new MvxRelayCommand(() => RequestClose(this));
            }
        }
    }
}