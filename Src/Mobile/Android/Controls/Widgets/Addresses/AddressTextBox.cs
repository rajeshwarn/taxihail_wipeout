using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Drawing;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls.Behavior;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses
{
    public class AddressTextBox : MvxFrameControl
    {
        public AddressTextBox(Context c, IAttributeSet attr ) : base (Resource.Layout.Control_AddressTextBox1, c,attr)
        {
            SetBehavior();
            //StreetNumberTextView.ApplyStyle(EditTextStyle.EditTextStyles.AddressBarStreetNumber);
            //AddressTextView.ApplyStyle(EditTextStyle.EditTextStyles.AddressBarAddress);
            IsReadOnly = false;
        }

        private AddressPickerViewModel ViewModel { get { return (AddressPickerViewModel)DataContext; } }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<AddressTextBox, AddressPickerViewModel>();
            set.Apply();
        }

        public EditText StreetNumberTextView
        {
            get { return (EditText)FindViewById<EditText>(Resource.Id.StreetNumberTextView); }
        }

        public  EditText AddressTextView
        {
            get { return (EditText)FindViewById<EditText>(Resource.Id.AddressTextView); }
        }

        public Action<string,string> AddressUpdated
        {
            get;
            set;
        }

        public View VerticalDivider
        {
            get { return FindViewById<View>(Resource.Id.VerticalDivider); }
        }
        public LinearLayout LoadingWheel
        {
            get { return FindViewById<LinearLayout>(Resource.Id.progressBar); }
        }

        public ImageView GreenDot
        {
            get { return FindViewById<ImageView>(Resource.Id.GreenDot); }
        }


        public string Address
        {
            get{
                return AddressTextView.Text;
            }
            set{
                AddressTextView.Text = value;
            }

        }

        public string Hint
        {
            set {
                AddressTextView.Hint = value;
            }
        }

        bool _isLoadingAddress;
        public bool IsLoadingAddress
        {
            get
            {
                return _isLoadingAddress;
            }
            set{
                _isLoadingAddress = value;
                if (value && !IsReadOnly)
                {
                    ShowLoadingWheel();
                }
                else
                {
                    HideLoadingWheel();
                }
            }
        }

        public void ShowLoadingWheel()
        {
            LoadingWheel.Visibility = ViewStates.Visible;
            StreetNumberTextView.ClearFocus();
            StreetNumberTextView.Visibility = ViewStates.Gone;
        }

        public void HideLoadingWheel()
        {
            LoadingWheel.Visibility = ViewStates.Gone;
            StreetNumberTextView.Visibility = IsReadOnly.ToVisibility(true);
        }
                
        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get
            {
                return _isReadOnly;
            }        
            set
            {
                _isReadOnly = value;

                VerticalDivider.Visibility = StreetNumberTextView.Visibility = value.ToVisibility(true);
 
                Resize();
            }
        }

        private void SetBehavior()
        {
            //Order is important
            NumberAndAddressTextFieldBehavior.ApplyTo(AddressTextView, StreetNumberTextView, (s,n) => 
                                                      {
                if ( AddressUpdated != null )
                {
                    AddressUpdated( s,n );
                }
            });
             
            StreetNumberTextView.FocusChange+= (sender, e) => {
                if(e.HasFocus)
                {
                    //StreetNumberTextView.SetTextColor(Color.White);
                    //StreetNumberTextView.SetHintTextColor(Color.White);
                    //AppFont.AddressBar.StreetNumberEditing.ApplyTo(StreetNumberTextView);
                    Resize();
                    VerticalDivider.Visibility = ViewStates.Gone;
                    if(_giantInvisibleButton !=null)
                    {
                        _giantInvisibleButton.Visibility = ViewStates.Visible;
                    }
                }
                else
                {                    
                    //StreetNumberTextView.SetTextColor( AppColors.AddressBar.TextColor.ToColor());
                    //StreetNumberTextView.SetHintTextColor(AppColors.AddressBar.TextColor.ToColor());
                    //AppFont.AddressBar.StreetNumber.ApplyTo(StreetNumberTextView);
                    Resize();
                    VerticalDivider.Visibility = ViewStates.Visible;
                    if(_giantInvisibleButton !=null)
                    {
                        _giantInvisibleButton.Visibility = ViewStates.Gone;
                    }
                }
            };
        }

        Button _giantInvisibleButton;
        public void SetInvisilbleButton(Button giantInvisibleButton)
        {
            giantInvisibleButton.Touch += (sender, e) => {
                if(e.Event.Action == MotionEventActions.Up)
                {
                    StreetNumberTextView.ClearFocus();
                }
            };
            _giantInvisibleButton = giantInvisibleButton;
        }

        void Resize()
        {
            AddressTextView.Enabled = !IsReadOnly;
            if (IsReadOnly)
            {
                StreetNumberTextView.Visibility = ViewStates.Gone;
                GreenDot.Visibility = ViewStates.Visible;
            }
            else
            {
                StreetNumberTextView.Visibility = ViewStates.Visible;
                GreenDot.Visibility = ViewStates.Gone;
            }
        }
    }
}

