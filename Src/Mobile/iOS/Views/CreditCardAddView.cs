using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class CreditCardAddView : BaseViewController
    {
        public CreditCardAddView () 
			: base("CreditCardAddView", null)
        {
        }
		
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            scrollView.ContentSize = new SizeF(scrollView.ContentSize.Width, txtSecurityCode.Frame.Bottom + 200);
			
            NavigationItem.Title = Resources.GetValue("CreditCardsAddTitle");

            var save = new UIBarButtonItem(UIBarButtonSystemItem.Save, null, null);
            save.Clicked += (sender, e) => ViewModel.AddCreditCardCommand.Execute();
            NavigationItem.RightBarButtonItem = save;

            NavigationItem.HidesBackButton = false;
            NavigationItem.BackBarButtonItem = new UIBarButtonItem(Resources.GetValue("BackButton"), UIBarButtonItemStyle.Bordered, null, null);

            lblNameOnCard.Text = Resources.GetValue("CreditCardName");
            lblCardNumber.Text = Resources.GetValue("CreditCardNumber");
            lblCardCategory.Text = Resources.GetValue("CreditCardCategory");
            lblExpMonth.Text = Resources.GetValue("CreditCardExpMonth");
            lblExpYear.Text = Resources.GetValue("CreditCardExpYear");
            lblSecurityCode.Text = Resources.GetValue("CreditCardCCV");

            txtNameOnCard.ShouldReturn += GoToNext;           

            ViewModel.CreditCardCompanies[0].Image = "Assets/CreditCard/visa.png";
            ViewModel.CreditCardCompanies[1].Image = "Assets/CreditCard/mastercard.png";
            ViewModel.CreditCardCompanies[2].Image = "Assets/CreditCard/amex.png";
            ViewModel.CreditCardCompanies[3].Image = "Assets/CreditCard/visa_electron.png";
			ViewModel.CreditCardCompanies[4].Image = "Assets/CreditCard/credit_card_generic.png";


// ReSharper disable CoVariantArrayConversion
            pickerCreditCardCategory.Configure(Resources.GetValue("CreditCardCategory"), ViewModel.CardCategories.ToArray(), ViewModel.CreditCardCategory , x=> {

                ViewModel.CreditCardCategory =  x.Id.GetValueOrDefault(); });

            pickerExpirationYear.Configure(Resources.GetValue("CreditCardExpYear"), ViewModel.ExpirationYears.ToArray(), ViewModel.ExpirationYear, x=> {
                ViewModel.ExpirationYear = x.Id;
            });
            

            (pickerExpirationMonth).Configure(Resources.GetValue("CreditCardExpMonth"), ViewModel.ExpirationMonths.ToArray(), ViewModel.ExpirationMonth, x=> {
                ViewModel.ExpirationMonth = x.Id;
            });
// ReSharper restore CoVariantArrayConversion
            this.AddBindings(new Dictionary<object, string>{
                { txtNameOnCard, "{'Text': {'Path': 'Data.NameOnCard', 'Mode': 'TwoWay' }}" }, 
				{ txtCardNumber, "{'Text': {'Path': 'CreditCardNumber', 'Mode': 'TwoWay' }, 'ImageLeftSource': {'Path': 'CreditCardImagePath'}}" }, 
                { pickerCreditCardCategory, "{'Text': {'Path': 'CreditCardCategoryName', 'Mode': 'TwoWay' }}" }, 
                { pickerExpirationMonth, "{'Text': {'Path': 'ExpirationMonthDisplay'}}" }, 
                { pickerExpirationYear, "{'Text': {'Path': 'ExpirationYear' }}" }, 
                { txtSecurityCode, "{'Text': {'Path': 'Data.CCV', 'Mode': 'TwoWay' }}" }
            });
         
            View.ApplyAppFont();     
        }

        private bool GoToNext (UITextField textField)
        {
            txtNameOnCard.ResignFirstResponder ();
            txtCardNumber.BecomeFirstResponder();
            return true;

        }
    }
}


