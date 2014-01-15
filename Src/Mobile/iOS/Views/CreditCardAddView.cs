using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreditCardAddView : BaseViewController<CreditCardAddViewModel>
    {
        public CreditCardAddView () 
			: base("CreditCardAddView", null)
        {
        }
		
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            scrollView.ContentSize = new SizeF(scrollView.ContentSize.Width, txtSecurityCode.Frame.Bottom + 200);
			
            NavigationItem.Title = Localize.GetValue("CreditCardsAddTitle");

            var save = new UIBarButtonItem(UIBarButtonSystemItem.Save, null, null);
            save.Clicked += (sender, e) => ViewModel.AddCreditCardCommand.Execute();
            NavigationItem.RightBarButtonItem = save;

            NavigationItem.HidesBackButton = false;
            NavigationItem.BackBarButtonItem = new UIBarButtonItem(Localize.GetValue("BackButton"), UIBarButtonItemStyle.Bordered, null, null);

            lblNameOnCard.Text = Localize.GetValue("CreditCardName");
            lblCardNumber.Text = Localize.GetValue("CreditCardNumber");
            lblCardCategory.Text = Localize.GetValue("CreditCardCategory");
            lblExpMonth.Text = Localize.GetValue("CreditCardExpMonth");
            lblExpYear.Text = Localize.GetValue("CreditCardExpYear");
            lblSecurityCode.Text = Localize.GetValue("CreditCardCCV");

            txtNameOnCard.ShouldReturn += GoToNext;           

            ViewModel.CreditCardCompanies[0].Image = "Assets/CreditCard/visa.png";
            ViewModel.CreditCardCompanies[1].Image = "Assets/CreditCard/mastercard.png";
            ViewModel.CreditCardCompanies[2].Image = "Assets/CreditCard/amex.png";
            ViewModel.CreditCardCompanies[3].Image = "Assets/CreditCard/visa_electron.png";
			ViewModel.CreditCardCompanies[4].Image = "Assets/CreditCard/credit_card_generic.png";


// ReSharper disable CoVariantArrayConversion
			pickerCreditCardCategory.Configure(Localize.GetValue("CreditCardCategory"), ViewModel.CardCategories.ToArray(), ViewModel.CreditCardCategory, x => { 
				ViewModel.CreditCardCategory =  x.Id.GetValueOrDefault(); 
			});

			pickerExpirationYear.Configure(Localize.GetValue("CreditCardExpYear"), ViewModel.ExpirationYears.ToArray(), ViewModel.ExpirationYear, x => {
                ViewModel.ExpirationYear = x.Id;
            });

            (pickerExpirationMonth).Configure(Localize.GetValue("CreditCardExpMonth"), ViewModel.ExpirationMonths.ToArray(), ViewModel.ExpirationMonth, x=> {
                ViewModel.ExpirationMonth = x.Id;
            });
// ReSharper restore CoVariantArrayConversion

			var set = this.CreateBindingSet<CreditCardAddView, CreditCardAddViewModel>();

			set.Bind(txtNameOnCard)
				.For(v => v.Text)
				.To(vm => vm.Data.NameOnCard);

			set.Bind(txtCardNumber)
				.For(v => v.Text)
				.To(vm => vm.CreditCardNumber);
			set.Bind(txtCardNumber)
				.For(v => v.ImageLeftSource)
				.To(vm => vm.CreditCardImagePath);

			set.Bind(pickerCreditCardCategory)
				.For("Text")
				.To(vm => vm.CreditCardCategoryName);

			set.Bind(pickerExpirationMonth)
				.For("Text")
				.To(vm => vm.ExpirationMonthDisplay);

			set.Bind(pickerExpirationYear)
				.For("Text")
				.To(vm => vm.ExpirationYear);

			set.Bind(txtSecurityCode)
				.For(v => v.Text)
				.To(vm => vm.Data.CCV);

			set.Apply ();

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


