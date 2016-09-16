using System;
using UIKit;
using CoreGraphics;
using CrossUI.Touch.Dialog.Elements;
using apcurium.MK.Common;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Foundation;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("CountrySelector")]
	public class CountrySelector:UILabel
	{
		UINavigationController _navigationController;
		Action<CountryCode> OnDialCodeChanged;

        public event PhoneNumberModel.PhoneNumberDatasourceChangedEventHandler NotifyChanges;
		Action PhoneNumberInfoDatasourceChanged;


        public CountrySelector(IntPtr handle):base(handle)
		{
			Initialize();
		}

        public CountrySelector():base()
		{
			Initialize();
		}

        public CountrySelector(CGRect frame):base(frame)
		{
			Initialize();
		}

		public void Initialize()
		{
			UserInteractionEnabled = true;
			var gr = new UITapGestureRecognizer(OnDialCodeSelectorClick);
			AddGestureRecognizer(gr);
		}
			
		public void Configure(UINavigationController navigationController)
		{
			_navigationController = navigationController;
		}

		public void Configure(UINavigationController navigationController, CountryCode selectedCountryCode, Action<CountryCode> onDialCodeChanged)
		{
			_navigationController = navigationController;
			OnDialCodeChanged = onDialCodeChanged;
			_selectedCountryCode = selectedCountryCode;

			if (PhoneNumberInfoDatasourceChanged != null)
			{
				PhoneNumberInfoDatasourceChanged();
			}
		}

        public void Configure(UINavigationController navigationController, PhoneNumberModel phoneNumberInfo)
		{
			_navigationController = navigationController;
			PhoneNumberInfoDatasourceChanged = phoneNumberInfo.PhoneNumberDatasourceChangedCallEvent;
			phoneNumberInfo.PhoneNumberDatasourceChanged += PhoneNumberDatasourceChanged;
			NotifyChanges += phoneNumberInfo.NotifyChanges;

			if (PhoneNumberInfoDatasourceChanged != null)
			{
				PhoneNumberInfoDatasourceChanged();
			}
		}

		CountryCode _selectedCountryCode;

		public CountryCode SelectedCountryCode
		{
			get
			{
				return _selectedCountryCode;
			}
			set
			{
				_selectedCountryCode = value;
				Text = value.CountryDialCode > 0 ? $"+{value.CountryDialCode}" : string.Empty;
			}
		}
			
		void PhoneNumberDatasourceChanged(object sender, PhoneNumberChangedEventArgs e)
		{
            SelectedCountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(e.Country));
		}

		public void OnDialCodeSelectorClick()
		{
			if (!Enabled)
			{
				return;
			}

			var section = new Section();

			foreach (var countryCode in CountryCode.CountryCodes)
			{
				// this is to generate spaces between dial code number field and country name to align country name in one column
				// the first column has different size between 0 and 4 characters (1 char for "+" and 3 for country dial code), according
				// to width of this first column the code inserts additional spaces to align second column (country names) vertically
				// in currently used font one caracter (A-Z a-z) equals to 2 spaces in its width

				var prefixSpacing = new string(' ', (Math.Max(3 - countryCode.CountryDialCode.ToString().Length, 0)) * 2);
				var text = countryCode.CountryDialCode != 0 
                  ? $"{prefixSpacing}+{countryCode.CountryDialCode} {countryCode.CountryName}"
                  : $"         {countryCode.CountryName}";

				var item = new RadioElementWithId<CountryCode>(countryCode, text, null, false, 15);

				item.Tapped += () =>
				{
					SelectedCountryCode = countryCode;

					if (OnDialCodeChanged != null)
					{
						OnDialCodeChanged(_selectedCountryCode);
					}

					if (NotifyChanges != null)
					{
                        NotifyChanges(null, new PhoneNumberChangedEventArgs() { Country = _selectedCountryCode.CountryISOCode });
					}
				};

				section.Add(item);
			}
				
            var rootElement = new RootElement(Localize.GetValue("DialCodeSelectorTitle"), new RadioGroup(CountryCode.GetCountryCodeIndexByCountryISOCode(SelectedCountryCode.CountryISOCode)));
			rootElement.Add(section);

			var dialCodeSelectorViewController = new TaxiHailDialogViewController(rootElement, true, false, 34);
			_navigationController.NavigationBar.Hidden = false;
			_navigationController.NavigationItem.BackBarButtonItem = new UIBarButtonItem(Localize.GetValue("BackButton"), UIBarButtonItemStyle.Bordered, null, null);
			_navigationController.PushViewController(dialCodeSelectorViewController, true);
		}
	}
}