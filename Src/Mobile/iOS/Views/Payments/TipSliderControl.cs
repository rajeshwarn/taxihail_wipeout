using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    [Register("TipSliderControl")]
    public partial class TipSliderControl : UIView
    {
        public TipSliderControl (IntPtr handle) :  base(handle)
        {
         
            BackgroundColor = UIColor.Clear;

			this.SetX (Frame.X - DeltaX)
				.SetWidth (Frame.Width + (2 * DeltaX));

            InitUiElements ();
            
			this.SetBottom(Frame.Top + _handle.Frame.Bottom);

            _handle.Dragged += (sender, e) =>  {
                Position += ((DraggedEventArgs)e).X;
                SetValue ();
                if (ValueChanged != null) {
                    ValueChanged.Invoke (this, new EventArgs ());
                }
            };

            MinValue = 0;
            MaxValue = 25;
            StepSize = 5;

            ValueChanged += (sender, e) =>  {
                Value = (int)(Math.Round (Value / StepSize) * StepSize);
            };
            
            
        }
        private const float HandleTouchWidth = 60; 
        private const float HandleTouchHeight = 60; 
        private const float HandleWidth = 21;        
        private const float HandleHeight = 42;
        static UIImage HandleImage = UIImage.FromFile("Assets/sliderHandle.png");
        
        static UIImage GrayBarEndImage = UIImage.FromFile("Assets/sliderGrayBar.png");
        static UIImage YellowBarEndImage = UIImage.FromFile("Assets/sliderYellowBar.png");
        static UIImage GrayBarBodyImage = UIImage.FromFile("Assets/sliderGrayBarBody.png");
        static UIImage YellowBarBodyImage = UIImage.FromFile("Assets/sliderYellowBarBody.png");

            
        DraggableButton _handle;
        ProgressBar _yellowBar;

        void InitUiElements ()
        {
            var barTop =((Frame.Height-HandleHeight)/2) +15;

			var grayBar= new ProgressBar(GrayBarEndImage,GrayBarBodyImage, new RectangleF(DeltaX,barTop,Width, HandleHeight));
            Add(grayBar);

			_yellowBar= new ProgressBar(YellowBarEndImage,YellowBarBodyImage, new RectangleF(DeltaX, barTop, Width/2, HandleHeight),true);            
            Add(_yellowBar);

            var barVertCenter = ((grayBar.Frame.Bottom - barTop)/2) + barTop;
            var handleTop = barVertCenter- (HandleTouchHeight/2);


            var count = 0;
            var strings = new[]{"0%","5%","10%","15%","20%","25%"};

            foreach(var str in strings)
            {
				var width = Width-25;
                var x = DeltaX+( ((width)/(strings.Length-1))*count);

                var label = new UILabel(new RectangleF(x,0,120,20));
                label.BackgroundColor = UIColor.Clear;
                label.TextColor = UIColor.Gray;
                label.Text = str;
                Add(label);

				x = DeltaX + ((WidthMinusHandle/(strings.Length-1))*count);
                var line = new UIView(new RectangleF(x+(HandleWidth/2),grayBar.Frame.Top+5f,1,+7));
                line.BackgroundColor = UIColor.Gray;
                Add (line);
                
                var line2 = new UIView(new RectangleF(x+(HandleWidth/2), grayBar.Frame.Bottom-5f,1,-7));
                line2.BackgroundColor = UIColor.Gray;
                Add (line2);

                count++;
            }
			          
            _handle = new DraggableButton (new RectangleF (0,handleTop, HandleTouchWidth, HandleTouchHeight));
            _handle.SetImage (HandleImage, UIControlState.Normal);
            Add (_handle);

        }

        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float StepSize { get; set; }

		private float Width {
			get{
				return Frame.Width-(2*DeltaX);
			}
		}

        private float WidthMinusHandle {
            get{
				return Width - HandleWidth;
            }
        }

        float DeltaX = (HandleTouchWidth - HandleWidth)/2;
        float DeltaY = (HandleTouchHeight - HandleHeight)/2;

        private float Position {
            get{
                return _handle.Frame.X + DeltaX;
            }
            set{
                var endPosition = value-DeltaX;

                _handle.Frame = _handle.Frame.SetX(endPosition);
                _yellowBar.Resize(value);
            }
        }

        private float _value;
        public float Value 
        { 
            get{
                return _value;
            }
            set{
                _value = value;                
                SetPosition();
            }
        }

        public bool Enabled {
            get{
                return _handle.Enabled;
            }
            set{
                _handle.Enabled = value;
            }
        }

        public event EventHandler ValueChanged;



        void LimitPosition()
        {
			if(Position < DeltaX)
            {
                Position = DeltaX;
            }
            if(Position > Width) {
                Position = Width;
            }
        }

        void SetPosition ()
        {
            var percent = (Value - MinValue)/MaxValue;
            Position = (percent *WidthMinusHandle)+DeltaX;

            LimitPosition();
        }

        void SetValue ()
        {
            var percent = Position/Width;
            Value = MinValue + (MaxValue*percent);

        }
    }
}

