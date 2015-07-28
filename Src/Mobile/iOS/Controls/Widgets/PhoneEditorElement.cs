using System;
using CrossUI.Touch.Dialog.Elements;
using apcurium.MK.Booking.Mobile.ViewModels;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Common;
using Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class PhoneEditorElement:ValueElement<PhoneNumberModel>
	{
        public event PhoneNumberModel.PhoneNumberDatasourceChangedEventHandler NotifyChanges;
		Action PhoneNumberInfoDatasourceChanged;

        CountrySelector phoneDialCodeLabel;
		UITextField phoneNumberTextEdit;
		UINavigationController navigationController;


        public PhoneEditorElement(string caption, PhoneNumberModel phoneNumberInfo, UINavigationController navigationController):base(caption, phoneNumberInfo)
		{
			PhoneNumberInfoDatasourceChanged = phoneNumberInfo.PhoneNumberDatasourceChangedCallEvent;
			phoneNumberInfo.PhoneNumberDatasourceChanged += PhoneNumberDatasourceChanged;
			this.NotifyChanges += phoneNumberInfo.NotifyChanges;
			this.navigationController = navigationController;
		}

		private static readonly NSString entryKey = new NSString("EntryElement");

		protected virtual NSString EntryKey
		{
			get { return entryKey; }
		}

		protected override UITableViewCell GetCellImpl(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(CellKey);
			if (cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			}
			else
				RemoveTag(cell, 1);

			GetCellImplInternal(tv, cell);

			cell.TextLabel.Text = Caption;
			return cell;
		}
			
		protected void GetCellImplInternal(UITableView tv, UITableViewCell cell)
		{
			float padding = 13.5f, dialCodeFieldLeftPadding = 60;
			CoreGraphics.CGRect rect = cell.Frame.SetX(padding + dialCodeFieldLeftPadding).SetWidth (UIScreen.MainScreen.Bounds.Width - 2 * 8 - 2 * padding - dialCodeFieldLeftPadding);// screenwidth - margin - padding

			phoneNumberTextEdit = new UITextField (rect);
			phoneNumberTextEdit.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin;
			phoneNumberTextEdit.Placeholder = "";
			phoneNumberTextEdit.Tag = 1;
			phoneNumberTextEdit.SecureTextEntry = false;
			phoneNumberTextEdit.TintColor = UIColor.Black;
			phoneNumberTextEdit.TextColor = UIColor.FromRGB(44, 44, 44);
			phoneNumberTextEdit.Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);
			phoneNumberTextEdit.VerticalAlignment = UIControlContentVerticalAlignment.Center;
			phoneNumberTextEdit.AdjustsFontSizeToFitWidth = true;
			phoneNumberTextEdit.AutocapitalizationType = UITextAutocapitalizationType.None;
			phoneNumberTextEdit.KeyboardType = UIKeyboardType.NumberPad;
			phoneNumberTextEdit.AdjustsFontSizeToFitWidth = true;
			phoneNumberTextEdit.SetHeight(21).IncrementY(11);
			phoneNumberTextEdit.EditingChanged += AfterTextChanged;

            phoneDialCodeLabel = new CountrySelector(cell.Frame.SetX(0).SetWidth(50));
			phoneDialCodeLabel.Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);
			phoneDialCodeLabel.TintColor = UIColor.Black;
			phoneDialCodeLabel.TextColor = UIColor.FromRGB(44, 44, 44);
			phoneDialCodeLabel.TextAlignment = UITextAlignment.Center;
			phoneDialCodeLabel.AdjustsFontSizeToFitWidth = true;
			phoneDialCodeLabel.SetHeight(phoneNumberTextEdit.Frame.Height).IncrementY(11).SetX(padding);
            phoneDialCodeLabel.Configure(navigationController, CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(Value.Country)), OnDialCodeSelected);

			cell.AddSubview(phoneDialCodeLabel);
			cell.AddSubview(phoneNumberTextEdit);

			PhoneNumberInfoDatasourceChanged();
		}
			
		void OnDialCodeSelected(CountryCode countryCode)
		{
            Value.Country = countryCode.CountryISOCode;

			if (NotifyChanges != null)
				NotifyChanges(this, new PhoneNumberChangedEventArgs()
					{
                        Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryDialCode(phoneDialCodeLabel.Text)).CountryISOCode,
						PhoneNumber = phoneNumberTextEdit.Text
					});
		}

		void AfterTextChanged(object sender, EventArgs e)
		{
			Value.PhoneNumber = phoneNumberTextEdit.Text;

			if (NotifyChanges != null)
				NotifyChanges(this, new PhoneNumberChangedEventArgs()
					{
                        Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryDialCode(phoneDialCodeLabel.Text)).CountryISOCode,
						PhoneNumber = phoneNumberTextEdit.Text
					});
		}
			
		void PhoneNumberDatasourceChanged(object sender, PhoneNumberChangedEventArgs e)
		{
			if (phoneDialCodeLabel != null && phoneNumberTextEdit != null)
			{
                phoneDialCodeLabel.SelectedCountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(Value.Country));
                phoneDialCodeLabel.Text = phoneDialCodeLabel.SelectedCountryCode.CountryDialCode > 0 ? "+" + phoneDialCodeLabel.SelectedCountryCode.CountryDialCode : null;
				phoneNumberTextEdit.Text = e.PhoneNumber;
			}
		}

		protected override void UpdateDetailDisplay(UITableViewCell cell)
		{
		}
	}
}