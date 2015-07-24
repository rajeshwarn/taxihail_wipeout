using System;
using UIKit;
using CoreGraphics;
using CrossUI.Touch.Dialog.Elements;
using apcurium.MK.Common;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Common.Entity;
using Foundation;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("CountrySelector")]
	public class CountrySelector:UILabel
	{
		UINavigationController navigationController;
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
			this.UserInteractionEnabled = true;
			UITapGestureRecognizer gr = new UITapGestureRecognizer(OnDialCodeSelectorClick);
			AddGestureRecognizer(gr);
		}
			
		public void Configure(UINavigationController navigationController)
		{
			this.navigationController = navigationController;
		}

		public void Configure(UINavigationController navigationController, CountryCode selectedCountryCode, Action<CountryCode> OnDialCodeChanged)
		{
			this.navigationController = navigationController;
			this.OnDialCodeChanged = OnDialCodeChanged;
			this.selectedCountryCode = selectedCountryCode;

            if (PhoneNumberInfoDatasourceChanged != null)
                PhoneNumberInfoDatasourceChanged();
		}

        public void Configure(UINavigationController navigationController, PhoneNumberModel phoneNumberInfo)
		{
			this.navigationController = navigationController;
			PhoneNumberInfoDatasourceChanged = phoneNumberInfo.PhoneNumberDatasourceChangedCallEvent;
			phoneNumberInfo.PhoneNumberDatasourceChanged += PhoneNumberDatasourceChanged;
			this.NotifyChanges += phoneNumberInfo.NotifyChanges;

            if (PhoneNumberInfoDatasourceChanged != null)
			    PhoneNumberInfoDatasourceChanged();
		}

		CountryCode selectedCountryCode;

		public CountryCode SelectedCountryCode
		{
			get
			{
				return selectedCountryCode;
			}

			set
			{
				selectedCountryCode = value;
				this.Text = value.CountryDialCode > 0 ? "+" + value.CountryDialCode.ToString() : null;
			}
		}
			
		void PhoneNumberDatasourceChanged(object sender, PhoneNumberChangedEventArgs e)
		{
            SelectedCountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(e.Country));
		}

		public void OnDialCodeSelectorClick()
		{
			var section = new Section();

			for (int i = 0; i < CountryCode.CountryCodes.Length; i++)
			{
				string text = "";

                // this is to generate spaces between dial code number field and country name to align country name in one column
                // the first column has different size between 0 and 4 characters (1 char for "+" and 3 for country dial code), according
                // to width of this first column the code inserts additional spaces to align second column (country names) vertically
                // in currently used font one caracter (A-Z a-z) equals to 2 spaces in its width
				text += CountryCode.CountryCodes[i].CountryDialCode != 0 ?
					new string(' ', (Math.Max(3 - CountryCode.CountryCodes[i].CountryDialCode.ToString().Length, 0)) * 2)
					+ "+" + CountryCode.CountryCodes[i].CountryDialCode
					: "        ";

				text += " " + CountryCode.CountryCodes[i].CountryName;

				var item = new RadioElementWithId<CountryCode>(CountryCode.CountryCodes[i], text, null, false, 15);

				CountryCode countryCode = CountryCode.CountryCodes[i];
				item.Tapped += () =>
				{
					SelectedCountryCode = countryCode;

					if (OnDialCodeChanged != null)
					{
						OnDialCodeChanged(selectedCountryCode);
					}

					if (NotifyChanges != null)
					{
                        NotifyChanges(null, new PhoneNumberChangedEventArgs() { Country = selectedCountryCode.CountryISOCode });
					}
				};

				section.Add(item);
			}
				
            RootElement rootElement = new RootElement(Localize.GetValue("DialCodeSelectorTitle"), new RadioGroup(CountryCode.GetCountryCodeIndexByCountryISOCode(SelectedCountryCode.CountryISOCode)));
			rootElement.Add(section);

			var dialCodeSelectorViewController = new TaxiHailDialogViewController(rootElement, true, false, 34);
			navigationController.NavigationBar.Hidden = false;
			navigationController.NavigationItem.BackBarButtonItem = new UIBarButtonItem(Localize.GetValue("BackButton"), UIBarButtonItemStyle.Bordered, null, null);
			navigationController.PushViewController(dialCodeSelectorViewController, true);
		}
	}
}