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
        Context context;
        CountryCode[] countryCodes;
        public CountryCodeAdapter(Context context, CountryCode[] countryCodes)
        {
            this.context = context;
            this.countryCodes = countryCodes;
        }

        public override int Count
        {
            get
            {
                return countryCodes.Length;
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
                return countryCodes[i];
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
                convertView = LayoutInflater.From(context).Inflate(Resource.Layout.CountryCodeItem, null);

            convertView.FindViewById<TextView>(Resource.Id.countryDialCodeItem).Text = countryCodes[position].GetTextCountryDialCode();

            return convertView;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
                convertView = LayoutInflater.From(context).Inflate(Resource.Layout.CountryCodeItemDropDown, null);

            convertView.FindViewById<TextView>(Resource.Id.countryDialCodeDropDownItem).Text = countryCodes[position].GetTextCountryDialCode();
            convertView.FindViewById<TextView>(Resource.Id.countryNameDropDownItem).Text = countryCodes[position].CountryName;

            return convertView;
        }
    }

    public class PhoneEditorElement : ValueElement<PhoneNumberModel>
    {
        public event PhoneNumberModel.PhoneNumberDatasourceChangedEventHandler NotifyChanges;

        Spinner phoneCountryCodeDropDown;
        EditText phoneNumberTextEdit;
        Action PhoneNumberInfoDatasourceChanged;

        public PhoneEditorElement(string caption, PhoneNumberModel phoneNumberInfo, string layoutName)
            : base(caption, phoneNumberInfo, layoutName)
		{
            PhoneNumberInfoDatasourceChanged = phoneNumberInfo.PhoneNumberDatasourceChangedCallEvent;
            phoneNumberInfo.PhoneNumberDatasourceChanged += PhoneNumberDatasourceChanged;
            this.NotifyChanges += phoneNumberInfo.NotifyChanges;
        }

        protected override View GetViewImpl(Context context, ViewGroup parent)
        {
            View phoneEditor = LayoutInflater.From(context).Inflate(Resource.Layout.PhoneEditor, null);
            phoneCountryCodeDropDown = phoneEditor.FindViewById(Resource.Id.countryDialCodeDropDown) as Spinner;
            phoneNumberTextEdit = phoneEditor.FindViewById(Resource.Id.phoneNumber) as EditText;

            phoneCountryCodeDropDown.Adapter = new CountryCodeAdapter(context, CountryCode.CountryCodes);

            phoneCountryCodeDropDown.ItemSelected += ItemSelected;
            phoneNumberTextEdit.AfterTextChanged += AfterTextChanged;

            PhoneNumberInfoDatasourceChanged();

            return phoneEditor;
        }

        protected override void UpdateDetailDisplay(View cell)
        {
        }

        void PhoneNumberDatasourceChanged(object sender, PhoneNumberChangedEventArgs e)
        {
            if (phoneCountryCodeDropDown != null && phoneNumberTextEdit != null)
            {
                phoneCountryCodeDropDown.SetSelection(CountryCode.GetCountryCodeIndexByCountryISOCode(e.Country));
                phoneNumberTextEdit.Text = e.PhoneNumber;
            }
        }

        void AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            if (NotifyChanges != null)
                NotifyChanges(this, new PhoneNumberChangedEventArgs()
                {
                    Country = CountryCode.GetCountryCodeByIndex(phoneCountryCodeDropDown.SelectedItemPosition).CountryISOCode,
                    PhoneNumber = phoneNumberTextEdit.Text
                });
        }

        void ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (NotifyChanges != null)
                NotifyChanges(this, new PhoneNumberChangedEventArgs()
                {
                    Country = CountryCode.GetCountryCodeByIndex(phoneCountryCodeDropDown.SelectedItemPosition).CountryISOCode,
                    PhoneNumber = phoneNumberTextEdit.Text
                });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}