using System;
using CrossUI.Touch.Dialog.Elements;
using apcurium.MK.Booking.Mobile.ViewModels;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Common;
using Foundation;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class PhoneEditorElement:ValueElement<PhoneNumberModel>
	{
        public event PhoneNumberModel.PhoneNumberDatasourceChangedEventHandler NotifyChanges;
	    private readonly Action _phoneNumberInfoDatasourceChanged;

        private CountrySelector _phoneDialCodeLabel;
		private UITextField _phoneNumberTextEdit;
		private readonly UINavigationController _navigationController;
	    private readonly string _placeHolder;


	    public PhoneEditorElement(string caption, PhoneNumberModel phoneNumberInfo, UINavigationController navigationController, string placeHolder = null):base(caption, phoneNumberInfo)
		{
			_phoneNumberInfoDatasourceChanged = phoneNumberInfo.PhoneNumberDatasourceChangedCallEvent;
			phoneNumberInfo.PhoneNumberDatasourceChanged += PhoneNumberDatasourceChanged;
			NotifyChanges += phoneNumberInfo.NotifyChanges;
			_navigationController = navigationController;
	        _placeHolder = placeHolder;
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

		    _phoneNumberTextEdit = new UITextField(rect)
		    {
		        AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin,
		        Placeholder = "",
		        Tag = 1,
		        SecureTextEntry = false,
		        TintColor = UIColor.Black,
		        TextColor = UIColor.FromRGB(44, 44, 44),
		        Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2),
		        VerticalAlignment = UIControlContentVerticalAlignment.Center,
		        AdjustsFontSizeToFitWidth = true,
		        AutocapitalizationType = UITextAutocapitalizationType.None,
		        KeyboardType = UIKeyboardType.NumberPad
		    };

            _phoneNumberTextEdit.Placeholder = _placeHolder;
		    _phoneNumberTextEdit.AdjustsFontSizeToFitWidth = true;
			_phoneNumberTextEdit.SetHeight(21).IncrementY(11);
			_phoneNumberTextEdit.EditingChanged += AfterTextChanged;
            _phoneNumberTextEdit.AccessibilityLabel = _phoneNumberTextEdit.Placeholder;

		    _phoneDialCodeLabel = new CountrySelector(cell.Frame.SetX(0).SetWidth(50))
		    {
		        Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2),
		        TintColor = UIColor.Black,
		        TextColor = UIColor.FromRGB(44, 44, 44),
		        TextAlignment = UITextAlignment.Center,
		        AdjustsFontSizeToFitWidth = true
		    };
		    _phoneDialCodeLabel.SetHeight(_phoneNumberTextEdit.Frame.Height).IncrementY(11).SetX(padding);
            _phoneDialCodeLabel.Configure(_navigationController, CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(Value.Country)), OnDialCodeSelected);
            _phoneDialCodeLabel.AccessibilityLabel = Localize.GetValue("DialCodeSelectorTitle");

			cell.AddSubview(_phoneDialCodeLabel);
			cell.AddSubview(_phoneNumberTextEdit);

			_phoneNumberInfoDatasourceChanged();
		}
			
		void OnDialCodeSelected(CountryCode countryCode)
		{
            Value.Country = countryCode.CountryISOCode;

			if (NotifyChanges != null)
				NotifyChanges(this, new PhoneNumberChangedEventArgs()
					{
                        Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryDialCode(_phoneDialCodeLabel.Text)).CountryISOCode,
						PhoneNumber = _phoneNumberTextEdit.Text
					});
		}

		void AfterTextChanged(object sender, EventArgs e)
		{
		    Value.PhoneNumber = _phoneNumberTextEdit.Text;

		    RaiseNotifyChange();
		}

	    private void RaiseNotifyChange()
	    {
	        if (NotifyChanges != null)
	        {
                NotifyChanges(this, new PhoneNumberChangedEventArgs()
                {
                    Country =
                          CountryCode.GetCountryCodeByIndex(
                              CountryCode.GetCountryCodeIndexByCountryDialCode(_phoneDialCodeLabel.Text)).CountryISOCode,
                    PhoneNumber = _phoneNumberTextEdit.Text
                });
            }
	    }

	    void PhoneNumberDatasourceChanged(object sender, PhoneNumberChangedEventArgs e)
		{
			if (_phoneDialCodeLabel != null && _phoneNumberTextEdit != null)
			{
                _phoneDialCodeLabel.SelectedCountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(Value.Country));
                _phoneDialCodeLabel.Text = _phoneDialCodeLabel.SelectedCountryCode.CountryDialCode > 0 ? "+" + _phoneDialCodeLabel.SelectedCountryCode.CountryDialCode : null;
				_phoneNumberTextEdit.Text = e.PhoneNumber;
			}
		}

		protected override void UpdateDetailDisplay(UITableViewCell cell)
		{
		}
	}
}