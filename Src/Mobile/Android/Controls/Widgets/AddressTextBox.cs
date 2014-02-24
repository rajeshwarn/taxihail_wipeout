using System;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Text;
using Android.Views.InputMethods;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using Android.Graphics;
using apcurium.MK.Booking.Mobile.Client.Controls.Behavior;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class AddressTextBox : LinearLayout
    {
        public Action<string,string> AddressUpdated { get; set; }

        private Color SelectedColor;
        public EditText AddressTextView;
        private EditText StreetNumberTextView;
        private LinearLayout LoadingWheel;
        private ImageView Dot;
        private View HorizontalDivider;

        public AddressTextBox(Context c, IAttributeSet attr) : base(c, attr)
        {

        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_AddressTextBox, this, true);

            SelectedColor = Resources.GetColor(Resource.Color.orderoptions_pickup_address_color);

            StreetNumberTextView = (EditText)layout.FindViewById(Resource.Id.StreetNumberTextView);
            AddressTextView = (EditText)layout.FindViewById(Resource.Id.AddressTextView);
            LoadingWheel = (LinearLayout)layout.FindViewById(Resource.Id.ProgressBar);
            Dot = (ImageView)layout.FindViewById(Resource.Id.Dot);
            HorizontalDivider = (View)layout.FindViewById(Resource.Id.HorizontalDivider);

            StreetNumberTextView.SetSelectAllOnFocus(true);
            StreetNumberTextView.SetSingleLine(true);
            StreetNumberTextView.Hint = "#";
            StreetNumberTextView.Gravity = GravityFlags.Center;
            StreetNumberTextView.InputType = StreetNumberTextView.InputType | InputTypes.ClassNumber;

            AddressTextView.SetSelectAllOnFocus(true);
            AddressTextView.SetSingleLine(true);
            AddressTextView.InputType = InputTypes.ClassText | InputTypes.TextFlagNoSuggestions;
            AddressTextView.ImeOptions = ImeAction.Go;

            SetBehavior();

            IsReadOnly = false;
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

        private bool? _isDestination;  //Needs to be nullable because by default it's false and the code in the setter was never called for the pickup.
        public bool IsDestination
        {
            get { return _isDestination.HasValue || _isDestination.Value; }        
            set
            {
                if ( !_isDestination.HasValue  || (_isDestination.Value != value))
                {
                    _isDestination = value;
                    if (value)
                    {
                        SelectedColor = Resources.GetColor(Resource.Color.orderoptions_destination_address_color);
                    }                       
                    Dot.SetColorFilter(SelectedColor, PorterDuff.Mode.SrcAtop);
                    HorizontalDivider.Visibility = value.ToVisibility();
                }
            }
        }

        private void ShowLoadingWheel()
        {
            LoadingWheel.Visibility = ViewStates.Visible;
            StreetNumberTextView.ClearFocus();
            StreetNumberTextView.Visibility = ViewStates.Gone;
        }

        private void HideLoadingWheel()
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

            StreetNumberTextView.FocusChange += (sender, e) => 
            {
                if(e.HasFocus)
                {
                    Resize();
                    StreetNumberTextView.SetTextColor(Color.White);
                    StreetNumberTextView.SetBackgroundColor(SelectedColor);
                    if(_giantInvisibleButton != null)
                    {
                        _giantInvisibleButton.Visibility = ViewStates.Visible;
                    }
                }
                else
                {                    
                    Resize();
                    StreetNumberTextView.SetTextColor(Resources.GetColor(Resource.Color.edit_text_foreground_color));
                    StreetNumberTextView.SetBackgroundColor(Color.Transparent);
                    if(_giantInvisibleButton != null)
                    {
                        _giantInvisibleButton.Visibility = ViewStates.Gone;
                    }
                }
            };
        }

        private Button _giantInvisibleButton;
        public void SetInvisibleButton(Button giantInvisibleButton)
        {
            giantInvisibleButton.Touch += (sender, e) => 
            {
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
                Dot.Visibility = ViewStates.Visible;
            }
            else
            {
                StreetNumberTextView.Visibility = ViewStates.Visible;
                Dot.Visibility = ViewStates.Gone;
            }
        }
    }
}

