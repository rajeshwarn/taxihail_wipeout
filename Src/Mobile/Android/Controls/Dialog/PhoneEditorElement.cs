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
using Android.Text;
using CrossUI.Droid.Dialog.Elements;
using apcurium.MK.Common;
using Cirrious.MvvmCross.Binding.Droid.Views;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Dialog
{
    public class CountryCodeAdapter : BaseAdapter<CountryCode>
    {
        private readonly Context _context;
        private readonly CountryCode[] _countryCodes;
        public CountryCodeAdapter(Context context, CountryCode[] countryCodes)
        {
            _context = context;
            _countryCodes = countryCodes;
        }

        public override int Count
        {
            get
            {
                return _countryCodes.Length;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override CountryCode this[int i]
        {
            get
            {
                return _countryCodes[i];
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
                convertView = LayoutInflater.From(_context).Inflate(Resource.Layout.CountryCodeItem, null);

            convertView.FindViewById<TextView>(Resource.Id.countryDialCodeItem).Text = _countryCodes[position].GetTextCountryDialCode();

            return convertView;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
                convertView = LayoutInflater.From(_context).Inflate(Resource.Layout.CountryCodeItemDropDown, null);

            convertView.FindViewById<TextView>(Resource.Id.countryDialCodeDropDownItem).Text = _countryCodes[position].GetTextCountryDialCode();
            convertView.FindViewById<TextView>(Resource.Id.countryNameDropDownItem).Text = _countryCodes[position].CountryName;

            return convertView;
        }
    }

    public class PhoneEditorElement : ValueElement<PhoneNumberModel>
    {
        public event PhoneNumberModel.PhoneNumberDatasourceChangedEventHandler NotifyChanges;

        private Spinner _phoneCountryCodeDropDown;
        private EditText _phoneNumberTextEdit;
        private readonly Action _phoneNumberInfoDatasourceChanged;

        public PhoneEditorElement(string caption, PhoneNumberModel phoneNumberInfo, string layoutName)
            : base(caption, phoneNumberInfo, layoutName)
		{
            _phoneNumberInfoDatasourceChanged = phoneNumberInfo.PhoneNumberDatasourceChangedCallEvent;
            phoneNumberInfo.PhoneNumberDatasourceChanged += PhoneNumberDatasourceChanged;
            this.NotifyChanges += phoneNumberInfo.NotifyChanges;
        }

        protected override View GetViewImpl(Context context, ViewGroup parent)
        {
            var phoneEditor = LayoutInflater.From(context).Inflate(Resource.Layout.PhoneEditor, null);
            _phoneCountryCodeDropDown = phoneEditor.FindViewById(Resource.Id.countryDialCodeDropDown) as Spinner;
            _phoneNumberTextEdit = phoneEditor.FindViewById(Resource.Id.phoneNumber) as EditText;

            _phoneCountryCodeDropDown.Adapter = new CountryCodeAdapter(context, CountryCode.CountryCodes);

            _phoneCountryCodeDropDown.ItemSelected += ItemSelected;
            _phoneNumberTextEdit.AfterTextChanged += AfterTextChanged;

            _phoneNumberInfoDatasourceChanged();

            return phoneEditor;
        }

        protected override void UpdateDetailDisplay(View cell)
        {
        }

        void PhoneNumberDatasourceChanged(object sender, PhoneNumberChangedEventArgs e)
        {
            if (_phoneCountryCodeDropDown != null && _phoneNumberTextEdit != null)
            {
                _phoneCountryCodeDropDown.SetSelection(CountryCode.GetCountryCodeIndexByCountryISOCode(e.Country));
                _phoneNumberTextEdit.Text = e.PhoneNumber;
            }
        }

        void AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            RaiseNotifyChange();
        }

        void ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            RaiseNotifyChange();
        }

        private void RaiseNotifyChange()
        {
            if (NotifyChanges != null)
            {
                NotifyChanges(this, new PhoneNumberChangedEventArgs()
                {
                    Country = CountryCode.GetCountryCodeByIndex(_phoneCountryCodeDropDown.SelectedItemPosition).CountryISOCode,
                    PhoneNumber = _phoneNumberTextEdit.Text
                });
            }
        }
    }
}