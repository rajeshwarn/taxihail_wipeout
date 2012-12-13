
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class CreditCardAddView : BaseViewController<CreditCardAddViewModel>
    {
        #region Constructors
        
        public CreditCardAddView () 
            : base(new MvxShowViewModelRequest<CreditCardAddViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public CreditCardAddView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public CreditCardAddView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }
        
        #endregion
		
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            scrollView.ContentSize = new SizeF(scrollView.ContentSize.Width, 416);
			
            View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background_full.png"));
            NavigationItem.Title = Resources.GetValue("CreditCardsAddTitle");

            var save = new UIBarButtonItem(UIBarButtonSystemItem.Save, null, null);
            save.Clicked += (sender, e) => ViewModel.AddCreditCardCommand.Execute();
            NavigationItem.RightBarButtonItem = save;

            NavigationItem.HidesBackButton = false;

            lblNameOnCard.Text = Resources.GetValue("CreditCardName");
            lblCardNumber.Text = Resources.GetValue("CreditCardNumber");
            lblCardCategory.Text = Resources.GetValue("CreditCardCategory");
            lblTypeCard.Text = Resources.GetValue("CreditCardType");
            lblExpMonth.Text = Resources.GetValue("CreditCardExpMonth");
            lblExpYear.Text = Resources.GetValue("CreditCardExpYear");
            lblSecurityCode.Text = Resources.GetValue("CreditCardCCV");
            lblZipCode.Text = Resources.GetValue("CreditCardZipCode");

            txtNameOnCard.ShouldReturn += GoToNext;
            txtCardNumber.ShouldReturn += GoToNext;
            pickerCreditCardCategory.ShouldReturn += GoToNext;
            txtTypeCard.ShouldReturn += GoToNext;
            txtExpMonth.ShouldReturn += GoToNext;
            txtExpYear.ShouldReturn += GoToNext;
            txtSecurityCode.ShouldReturn += GoToNext;
            txtZipCode.ShouldReturn += GoToNext;

            ((ModalTextField)pickerCreditCardCategory).Configure(Resources.GetValue("CreditCardCategory"), ViewModel.CardCategories.ToArray(), ViewModel.CreditCardCategory , x=> {
                ViewModel.CreditCardCategory =  x.Id; });

            this.AddBindings(new Dictionary<object, string>{
                { txtNameOnCard, "{'Text': {'Path': 'Data.NameOnCard', 'Mode': 'TwoWay' }}" }, 
                { txtCardNumber, "{'Text': {'Path': 'Data.CardNumber', 'Mode': 'TwoWay' }}" }, 
                { pickerCreditCardCategory, "{'Text': {'Path': 'CreditCardCategoryName', 'Mode': 'TwoWay' }}" }, 
                { txtTypeCard, "{'Text': {'Path': 'Data.CreditCardCompany', 'Mode': 'TwoWay' }}" }, 
                { txtExpMonth, "{'Text': {'Path': 'Data.ExpirationMonth', 'Mode': 'TwoWay' }}" }, 
                { txtExpYear, "{'Text': {'Path': 'Data.ExpirationYear', 'Mode': 'TwoWay' }}" }, 
                { txtSecurityCode, "{'Text': {'Path': 'Data.CCV', 'Mode': 'TwoWay' }}" }, 
                { txtZipCode, "{'Text': {'Path': 'Data.ZipCode', 'Mode': 'TwoWay' }}" }, 
            });

            this.View.ApplyAppFont();     
        }

        private bool GoToNext (UITextField textField)
        {
            textField.ResignFirstResponder ();

            if (textField == txtNameOnCard) {
                txtCardNumber.BecomeFirstResponder();
            }else if (textField == txtCardNumber) {
                pickerCreditCardCategory.BecomeFirstResponder();
            }else if (textField == pickerCreditCardCategory) {
                txtTypeCard.BecomeFirstResponder();
            }else if (textField == txtTypeCard) {
                txtExpMonth.BecomeFirstResponder();
            }else if (textField == txtExpMonth) {
                txtExpYear.BecomeFirstResponder();
            }else if (textField == txtExpYear) {
                txtSecurityCode.BecomeFirstResponder();
            }else if (textField == txtSecurityCode) {
                txtZipCode.BecomeFirstResponder();
            }

            return true;

        }
    }
}

