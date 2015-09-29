using System;
using apcurium.MK.Booking.Mobile.Client.Controls.Behavior;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.mk.booking.mobile.client.controls.widgets.AddressTextBox")]
    public class AddressTextBox : LinearLayout
    {
        public Action<string> AddressUpdated { get; set; }

        public event EventHandler AddressClicked;

        private Color _selectedColor;
		public EditTextWithAccessibility AddressTextView;
		private EditTextWithAccessibility _streetNumberTextView;
        private LinearLayout _loadingWheel;
        private ImageView _dot;
        private View _horizontalDivider;

		ILocalization _localization = TinyIoCContainer.Current.Resolve<ILocalization>();

        public AddressTextBox(Context c, IAttributeSet attr) : base(c, attr)
        {

        }



        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_AddressTextBox, this, true);

            _selectedColor = Resources.GetColor(Resource.Color.orderoptions_pickup_address_color);

			_streetNumberTextView = (EditTextWithAccessibility)layout.FindViewById(Resource.Id.StreetNumberTextView);
			AddressTextView = (EditTextWithAccessibility)layout.FindViewById(Resource.Id.AddressTextView);
            _loadingWheel = (LinearLayout)layout.FindViewById(Resource.Id.ProgressBar);
            _dot = (ImageView)layout.FindViewById(Resource.Id.Dot);
            _horizontalDivider = layout.FindViewById(Resource.Id.HorizontalDivider);

            _streetNumberTextView.SetSelectAllOnFocus(true);
            _streetNumberTextView.ImeOptions = ImeAction.Done;
            _streetNumberTextView.SetSingleLine(true);
            _streetNumberTextView.Hint = "#";
            _streetNumberTextView.Gravity = GravityFlags.Center;
            _streetNumberTextView.InputType = _streetNumberTextView.InputType | InputTypes.ClassNumber;
			_streetNumberTextView.ContentDescription = ContentDescription + " " + _localization["StreetNumber"];

            AddressTextView.SetSelectAllOnFocus(true);
            AddressTextView.SetSingleLine(true);
            AddressTextView.InputType = InputTypes.ClassText | InputTypes.TextFlagNoSuggestions;
            AddressTextView.ImeOptions = ImeAction.Go;
			AddressTextView.Hint = ContentDescription;
			AddressTextView.ContentDescription = ContentDescription;


            SetBehavior();

            IsSelected = true;
        }

	    public bool UserInputDisabled
	    {
		    get { return _userInputDisabled; }
		    set
		    {
			    _userInputDisabled = value;

			    _streetNumberTextView.Enabled = !_userInputDisabled;
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
                if (value != _isLoadingAddress)
                {
                    _isLoadingAddress = value;

                    if (_isLoadingAddress && IsSelected)
                    {
                        ShowLoadingWheel();
                    }
                    else
                    {
                        HideLoadingWheel();
                    }
                }
            }
        }

        private bool? _isDestination;  //Needs to be nullable because by default it's false and the code in the setter was never called for the pickup.
        public bool IsDestination
        {
            get { return _isDestination.HasValue && _isDestination.Value; }        
            set
            {
                if ( !_isDestination.HasValue  || (_isDestination.Value != value))
                {
                    _isDestination = value;
                    if (value)
                    {
                        _selectedColor = Resources.GetColor(Resource.Color.orderoptions_destination_address_color);
                    }                       
                    _dot.SetColorFilter(_selectedColor, PorterDuff.Mode.SrcAtop);
                    _horizontalDivider.Visibility = value.ToVisibility();
                }
            }
        }

        private void ShowLoadingWheel()
        {
            _loadingWheel.Visibility = ViewStates.Visible;
            _streetNumberTextView.ClearFocus();
            _streetNumberTextView.LayoutParameters.Width = 0; //not using visibility to avoid triggering focus change
        }

        private void HideLoadingWheel()
        {
            _loadingWheel.Visibility = ViewStates.Gone;
            //not using visibility to avoid triggering focus change
            _streetNumberTextView.LayoutParameters.Width = IsSelected 
                ? ViewGroup.LayoutParams.WrapContent 
                : 0;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }        
            set
            {
                _isSelected = value;
                Resize();
            }
        }

        private void SetBehavior()
        {
            //Order is important
            NumberAndAddressTextFieldBehavior.ApplyTo(AddressTextView, _streetNumberTextView, number => 
            {
                if (AddressUpdated != null)
                {
                    AddressUpdated(number);
                }
            });

            AddressTextView.Click += (sender, e) => 
            {
                if(!UserInputDisabled && AddressClicked!= null)
                {
                    AddressClicked(this, EventArgs.Empty);
                }
            };

            _streetNumberTextView.FocusChange += (sender, e) => 
            {
                if(e.HasFocus)
                {
                    if(string.IsNullOrWhiteSpace(_streetNumberTextView.Text))
                    {
                        _streetNumberTextView.ClearFocus();
                        if(AddressClicked != null)
                        {
                            AddressClicked(this, EventArgs.Empty);
                        }
                        return;
                    }

                    Resize();
                    _streetNumberTextView.SetTextColor(Color.White);
                    _streetNumberTextView.SetBackgroundColor(_selectedColor);
                    if(_giantInvisibleButton != null)
                    {
                        _giantInvisibleButton.Visibility = ViewStates.Visible;
                    }
                }
                else
                {                    
                    Resize();
                    _streetNumberTextView.SetTextColor(Resources.GetColor(Resource.Color.edit_text_foreground_color));
                    _streetNumberTextView.SetBackgroundColor(Color.Transparent);
                    if(_giantInvisibleButton != null)
                    {
                        _giantInvisibleButton.Visibility = ViewStates.Gone;
                    }
                }
            };
        }

        private Button _giantInvisibleButton;
	    private bool _userInputDisabled;

	    public void SetInvisibleButton(Button giantInvisibleButton)
        {
            giantInvisibleButton.Touch += (sender, e) => 
            {
                if(e.Event.Action == MotionEventActions.Up)
                {
                    _streetNumberTextView.ClearFocus();
                }
            };

            _giantInvisibleButton = giantInvisibleButton;
        }

        void Resize()
        {
            if (!IsSelected)
            {
                //not using visibility to avoid triggering focus change
                _streetNumberTextView.LayoutParameters.Width = 0;
                _dot.Visibility = ViewStates.Visible;
            }
            else
            {
                //not using visibility to avoid triggering focus change
                _streetNumberTextView.LayoutParameters.Width = ViewGroup.LayoutParams.WrapContent;
                _dot.Visibility = ViewStates.Gone;
            }
        }
    }
}

