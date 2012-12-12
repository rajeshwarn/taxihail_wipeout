using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PaymentPreferenceViewModel : BaseViewModel,
        IMvxServiceConsumer<IAccountService>
    {

        private ListItem[] _paymentList;

        public ListItem[] PaymentList
        {
            get { return _paymentList; }
            set { this._paymentList = value; }
        }

        private double? _defaultTipAmount;

        public double? DefaultTipAmount
        {
            get { return this._defaultTipAmount; }
            set { this._defaultTipAmount = value; }
        }

        private double? _defaultTipPercent;

        public double? DefaultTipPercent
        {
            get { return this._defaultTipPercent; }
            set { this._defaultTipPercent = value; }
        }

        public PaymentPreferenceViewModel()
        {
            var accountService = this.GetService<IAccountService>();
            DefaultTipAmount = accountService.CurrentAccount.DefaultTipAmount;
            DefaultTipPercent = accountService.CurrentAccount.DefaultTipPercent;
            PaymentList = accountService.GetPaymentsList().ToArray();
        }

        public int PaymentTypeId
        {
            get
            {
                if (PaymentList.Any())
                {
                    return PaymentList[0].Id;
                }
                return 0;
            }
            set
            {
                if (value != PaymentTypeId)
                {
                    PaymentTypeId = value;
                    FirePropertyChanged("PaymentTypeId");
                    FirePropertyChanged("PaymentTypeName");
                }
            }
        }

        public string PaymentTypeName
        {
            get
            {
                var paymentType = this.PaymentList.FirstOrDefault(x => x.Id == PaymentTypeId);
                if (paymentType == null) return null;
                return paymentType.Display;
            }
        }

        public IMvxCommand ConfirmPreference
        {
            get
            {
                return GetCommand(this.Close);
            }
        }
    }
}