using System;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Common.Extensions;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("AddressBar")]
	public class AddressBar : UIView
	{
		private TextField _addressTextView;
		private VerticalButtonBar _bar;

		public event EventHandler Started;
		public event EventHandler Ended;
		public event EventHandler EditingChanged;
		public delegate void BarItemClickedHandler( int index );
		public event BarItemClickedHandler BarItemClicked;

		public AddressBar ()
		{
			Initialize();
		}
		
		public AddressBar (IntPtr handle) : base(  handle )
		{
			Initialize();
		}

		private void Initialize()
		{
			var rect = Frame;

			_addressTextView = new TextField( new RectangleF( 9, 6, 253, rect.Height - 12 ) );
			_addressTextView.Font = AppStyle.NormalTextFont;
			_addressTextView.TextColor = AppStyle.GreyText;
			_addressTextView.PaddingLeft = 3;
			_addressTextView.VerticalAlignment = UIControlContentVerticalAlignment.Center;
			
			_addressTextView.ReturnKeyType = UIReturnKeyType.Search;
			_addressTextView.AutocorrectionType = UITextAutocorrectionType.No;
			_addressTextView.AutocapitalizationType = UITextAutocapitalizationType.None;
			_addressTextView.ShouldReturn = delegate(UITextField textField)
			{
				return _addressTextView.ResignFirstResponder();
			};
			
			_addressTextView.Started += HandleStarted;
			_addressTextView.Ended += HandleEnded;
			_addressTextView.EditingChanged += HandleEditingChanged;
			
			_bar = new VerticalButtonBar( new RectangleF( _addressTextView.Frame.Right + 9, rect.Height/2 - 33/2, 40, 33 ), VerticalButtonBar.AnimationType.Arrow, apcurium.MK.Booking.Mobile.Client.VerticalButtonBar.AnimationDirection.Down );
			_bar.AddButton(UIImage.FromFile("Assets/VerticalButtonBar/locationIcon.png"), UIImage.FromFile("Assets/VerticalButtonBar/locationIcon.png"));
			_bar.AddButton(UIImage.FromFile("Assets/VerticalButtonBar/favoriteIcon.png"), UIImage.FromFile("Assets/VerticalButtonBar/favoriteIcon.png"));
			_bar.AddButton(UIImage.FromFile("Assets/VerticalButtonBar/contacts.png"), UIImage.FromFile("Assets/VerticalButtonBar/contacts.png"));
			_bar.AddButton(UIImage.FromFile("Assets/VerticalButtonBar/nearbyIcon.png"), UIImage.FromFile("Assets/VerticalButtonBar/nearbyIcon.png"));

			_bar.ButtonClicked += HandleButtonClicked;


			AddSubview( _addressTextView );
			AddSubview( _bar );

			BackgroundColor = UIColor.Clear;

			BringSubviewToFront( _bar );
		}

		void HandleButtonClicked (int index)
		{
			BarItemClicked.Maybe( b => b( index ) );
		}

		void HandleEditingChanged (object sender, EventArgs e)
		{
			EditingChanged.Maybe( ec => ec( sender, e ) );
		}

		void HandleEnded (object sender, EventArgs e)
		{
			Ended.Maybe( ended => ended( sender, e ) );
		}

		void HandleStarted (object sender, EventArgs e)
		{
			Started.Maybe( s => s( sender, e ) );
		}

		public string Text { 
			get {
				return _addressTextView.Text;
			}
			set {
				_addressTextView.Text = value;
			}
		}

		public string Placeholder {
			get {
				return _addressTextView.Placeholder;
			}
			set {
				_addressTextView.Placeholder = value;
			}
		}

		public bool ClearBackground { get; set; }

		public override bool PointInside (PointF point, UIEvent uievent)
		{
			return this.Frame.Contains( point ) || _bar.Frame.Contains( point );
		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);

			var colorSpace = CGColorSpace.CreateDeviceRGB();
			var context = UIGraphics.GetCurrentContext();
			
			var newGradientColors = AppStyle.GetButtonColors( AppStyle.ButtonColor.Silver ).Select( c => c.CGColor ).ToArray();
			var newGradientLocations = AppStyle.GetButtonColorLocations( AppStyle.ButtonColor.Silver );
			var newGradient = new CGGradient(colorSpace, newGradientColors, newGradientLocations);

			var radius = 0;

			ShadowSetting dropShadow = null;
			var innerShadow = AppStyle.GetInnerShadow( AppStyle.ButtonColor.Silver );

			rect.Width -= dropShadow != null ? Math.Abs(dropShadow.Offset.Width) : 0;
			rect.Height -= dropShadow != null ? Math.Abs(dropShadow.Offset.Height) : 0;
			rect.X += dropShadow != null && dropShadow.Offset.Width < 0 ? Math.Abs(dropShadow.Offset.Width) : 0;
			rect.Y += dropShadow != null && dropShadow.Offset.Height < 0 ? Math.Abs(dropShadow.Offset.Height) : 0;


			var roundedRectanglePath = UIBezierPath.FromRoundedRect(rect, radius);
			if( !ClearBackground )
			{
				context.SaveState();
				if (dropShadow != null)
				{
					context.SetShadowWithColor(dropShadow.Offset, dropShadow.BlurRadius, dropShadow.Color.CGColor);
				}
				
				context.BeginTransparencyLayer(null);
				roundedRectanglePath.AddClip();
				context.DrawLinearGradient(newGradient, new PointF(rect.X + (rect.Width / 2.0f), rect.Y), new PointF(rect.X + (rect.Width / 2.0f), rect.Y + rect.Height), 0);
				context.EndTransparencyLayer();
				context.RestoreState();
			}

			if (innerShadow != null)
			{
				var roundedRectangleBorderRect = roundedRectanglePath.Bounds;
				roundedRectangleBorderRect.Inflate(innerShadow.BlurRadius, innerShadow.BlurRadius);
				roundedRectangleBorderRect.Offset(-innerShadow.Offset.Width, -innerShadow.Offset.Height);
				roundedRectangleBorderRect = RectangleF.Union(roundedRectangleBorderRect, roundedRectanglePath.Bounds);
				roundedRectangleBorderRect.Inflate(1, 1);
				
				var roundedRectangleNegativePath = UIBezierPath.FromRect(roundedRectangleBorderRect);
				roundedRectangleNegativePath.AppendPath(roundedRectanglePath);
				roundedRectangleNegativePath.UsesEvenOddFillRule = true;
				
				context.SaveState();
				{
					var xOffset = innerShadow.Offset.Width + (float)Math.Round(roundedRectangleBorderRect.Width);
					var yOffset = innerShadow.Offset.Height;
					context.SetShadowWithColor(
						new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
						innerShadow.BlurRadius,
						innerShadow.Color.CGColor);
					
					roundedRectanglePath.AddClip();
					var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(roundedRectangleBorderRect.Width), 0);
					roundedRectangleNegativePath.ApplyTransform(transform);
					UIColor.Gray.SetFill();
					roundedRectangleNegativePath.Fill();
				}
				context.RestoreState();
			}

			


		}
	}
}

