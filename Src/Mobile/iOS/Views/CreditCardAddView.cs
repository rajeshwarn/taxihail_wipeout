using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.Controls;

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
            lblTypeCard.Text = Resources.GetValue("CreditCardType");
            lblExpMonth.Text = Resources.GetValue("CreditCardExpMonth");
            lblExpYear.Text = Resources.GetValue("CreditCardExpYear");
            lblSecurityCode.Text = Resources.GetValue("CreditCardCCV");

            txtNameOnCard.ShouldReturn += GoToNext;           

            ViewModel.CreditCardCompanies[0].Image = "Assets/CreditCard/visa.png";
            ViewModel.CreditCardCompanies[1].Image = "Assets/CreditCard/mastercard.png";
            ViewModel.CreditCardCompanies[2].Image = "Assets/CreditCard/amex.png";
            ViewModel.CreditCardCompanies[3].Image = "Assets/CreditCard/visa_electron.png";


            ((ModalTextField)pickerCreditCardCategory).Configure(Resources.GetValue("CreditCardCategory"), ViewModel.CardCategories.ToArray(), ViewModel.CreditCardCategory , x=> {
                ViewModel.CreditCardCategory =  x.Id.GetValueOrDefault(); });

            ((ModalTextField)pickerCreditCardType).Configure(Resources.GetValue("CreditCardType"), ViewModel.CreditCardCompanies.ToArray(), ViewModel.CreditCardType , x=> {
                ViewModel.CreditCardType =  x.Id.GetValueOrDefault(); });

            ((ModalTextField)pickerExpirationYear).Configure(Resources.GetValue("CreditCardExpYear"), ViewModel.ExpirationYears.ToArray(), ViewModel.ExpirationYear, x=> {
                ViewModel.ExpirationYear = x.Id;
            });

            ((ModalTextField)pickerExpirationMonth).Configure(Resources.GetValue("CreditCardExpMonth"), ViewModel.ExpirationMonths.ToArray(), ViewModel.ExpirationMonth, x=> {
                ViewModel.ExpirationMonth = x.Id;
            });
                       
            this.AddBindings(new Dictionary<object, string>{
                { txtNameOnCard, "{'Text': {'Path': 'Data.NameOnCard', 'Mode': 'TwoWay' }}" }, 
                { txtCardNumber, "{'Text': {'Path': 'Data.CardNumber', 'Mode': 'TwoWay' }}" }, 
                { pickerCreditCardCategory, "{'Text': {'Path': 'CreditCardCategoryName', 'Mode': 'TwoWay' }}" }, 
                { pickerCreditCardType, "{'Text': {'Path': 'CreditCardTypeName', 'Mode': 'TwoWay' }, 'LeftImagePath' : {'Path': 'CreditCardImagePath'}}" }, 
                { pickerExpirationMonth, "{'Text': {'Path': 'ExpirationMonthDisplay'}}" }, 
                { pickerExpirationYear, "{'Text': {'Path': 'ExpirationYear' }}" }, 
                { txtSecurityCode, "{'Text': {'Path': 'Data.CCV', 'Mode': 'TwoWay' }}" }
            });
         
            this.View.ApplyAppFont();     
        }

        private bool GoToNext (UITextField textField)
        {
            txtNameOnCard.ResignFirstResponder ();
            txtCardNumber.BecomeFirstResponder();
            return true;

        }
    }
}


