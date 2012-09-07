using System;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Common.Extensions;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
	public class TwoLinesAddressCell : UITableViewCell
	{
		private UIImageView _rightImage;
		private TwoLinesAddressItem _sectionItem;
		
		public TwoLinesAddressCell (IntPtr handle) : base(handle)
		{		
		}

		[Export("initWithCoder:")]
		public TwoLinesAddressCell (NSCoder coder) : base(coder)
		{			
		}
		
		public TwoLinesAddressCell (TwoLinesAddressItem data, string cellIdentifier) : base( UITableViewCellStyle.Subtitle, cellIdentifier )
		{					
			_sectionItem = data;
			
			SelectionStyle = UITableViewCellSelectionStyle.None;
			Accessory = UITableViewCellAccessory.None;
			Initialize ();
			Load ();
		}
		
		private void Initialize ()
		{

			BackgroundView = new CustomCellBackgroundView( _sectionItem.Index == 0, _sectionItem.Index == (_sectionItem.Parent.Items.Count() - 1), Frame, _sectionItem.ShowPlusSign );
			TextLabel.TextColor = AppStyle.CellFirstLineTextColor;
			TextLabel.BackgroundColor = UIColor.Clear;
			TextLabel.Font = AppStyle.CellFont;

			DetailTextLabel.TextColor = AppStyle.CellSecondLineTextColor;
			DetailTextLabel.BackgroundColor = UIColor.Clear;
			DetailTextLabel.Font = AppStyle.NormalTextFont;

			_rightImage = new UIImageView (new RectangleF (290, _sectionItem.RowHeight/2 - 15/2, 14, 15 ) ); 
			_rightImage.BackgroundColor = UIColor.Clear;
			_rightImage.ContentMode = UIViewContentMode.ScaleAspectFit;
			AddSubview ( _rightImage );	
		}

		
		public void Load ()
		{
			TextLabel.Text = _sectionItem.Label;
			DetailTextLabel.Text = _sectionItem.DetailText;
			UserInteractionEnabled = _sectionItem.Enabled();

			if( _sectionItem.ShowRightArrow )
			{
				LoadImageFromAssets ( _rightImage, "Assets/Cells/rightArrow.png" );
			}
			if( _sectionItem.ShowPlusSign )
			{
				LoadImageFromAssets ( _rightImage, "Assets/Cells/plusSign.png" );
			}

		}
		
		public float GetHeight ()
		{
			return _sectionItem.RowHeight;
		}
		
		private void LoadImageFromAssets ( UIImageView imageView, string asset )
		{
			if ( !asset.IsNullOrEmpty () )
			{
				imageView.Image = UIImage. FromFile ( asset );
			}			
		}
		
		public void ReUse ( TwoLinesAddressItem item )
		{
			if ( item != null )
			{
				if ( item != _sectionItem )
				{								
					_sectionItem = item;
				}			

				Load ();
				bool changed = false;
				if( ((CustomCellBackgroundView)BackgroundView).IsAddNewCell != _sectionItem.ShowPlusSign )
				{
					((CustomCellBackgroundView)BackgroundView).IsAddNewCell = _sectionItem.ShowPlusSign;
					changed = true;
				}

				if( ((CustomCellBackgroundView)BackgroundView).IsTop != (_sectionItem.Index == 0) )
				{
					((CustomCellBackgroundView)BackgroundView).IsTop = _sectionItem.Index == 0;
					changed = true;
				}
				if( ((CustomCellBackgroundView)BackgroundView).IsBottom != (_sectionItem.Index == (_sectionItem.Parent.Items.Count() - 1)) )
				{
					((CustomCellBackgroundView)BackgroundView).IsBottom = _sectionItem.Index == (_sectionItem.Parent.Items.Count() - 1);
					changed = true;
				}
				if( changed )
				{
					BackgroundView.SetNeedsDisplay();
				}

			}

		}
		
		public void CleanUp ()
		{					
			_sectionItem = null;		
			
			if ( _rightImage != null )
			{
				_rightImage.Image = new UIImage ();
				_rightImage.Dispose ();
				_rightImage = null;
			}						
		}
	
		public override void TouchesBegan ( NSSet touches, UIEvent evt )
		{	
			BackgroundView.SetNeedsDisplay();
			base.TouchesBegan ( touches, evt );	
		}
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			SetHighlighted( false, true );
			BackgroundView.SetNeedsDisplay();
			base.TouchesEnded (touches, evt);
		}
		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			SetHighlighted( false, true );
			BackgroundView.SetNeedsDisplay();
			base.TouchesCancelled (touches, evt);
		}


	}

	public class CustomCellBackgroundView : UIView
	{
		private bool _isTop;
		private bool _isBottom;
		private float _cornerRadius = 3f;
		private float _strokeSize = 1f;
		private float _innerShadowTopBottomBlurRadius = 3f;
		private float _innerShadowSidesBlurRadius = 2f;
		private UIColor _strokeColor = UIColor.FromRGB( 105, 105, 105 );
		private UIColor _selectedBackgroundColor = UIColor.FromRGBA( 233, 217, 219, 0.1f );
		private UIColor _backgroundColor = AppStyle.CellBackgroundColor;

		public CustomCellBackgroundView(bool isTop, bool isBottom, RectangleF rect, bool isAddNewCell ) : base( rect )
		{
			_isTop = isTop;
			_isBottom = isBottom;
			BackgroundColor = UIColor.Clear;
			IsAddNewCell = isAddNewCell;

		}

		public bool IsAddNewCell { get; set; }

		public bool IsTop { 
			get { return _isTop; } 
			set { _isTop = value; }
		}

		public bool IsBottom { 
			get { return _isBottom; } 
			set { _isBottom = value; }
		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);

			var context = UIGraphics.GetCurrentContext();

			context.SetLineWidth( _strokeSize );

			UIBezierPath fillRectPath;
			UIBezierPath strokePath = new UIBezierPath();
			if( _isTop && _isBottom )
			{
				fillRectPath = UIBezierPath.FromRoundedRect( rect, _cornerRadius );
				strokePath = fillRectPath;
			}
			else if( _isTop )
			{
				fillRectPath = UIBezierPath.FromRoundedRect(rect, UIRectCorner.TopLeft | UIRectCorner.TopRight, new SizeF(_cornerRadius,_cornerRadius) );
				strokePath.MoveTo( new PointF( rect.Left, rect.Bottom ));
				strokePath.AddLineTo( new PointF( rect.Left, rect.Top + _cornerRadius ));
				strokePath.AddArc( new PointF( rect.Left + _cornerRadius, rect.Top + _cornerRadius), _cornerRadius, (float)Math.PI, (float)(3*Math.PI/2), true );
				strokePath.AddLineTo( new PointF( rect.Right - _cornerRadius, rect.Top ));
				strokePath.AddArc( new PointF( rect.Right - _cornerRadius, rect.Top + _cornerRadius), _cornerRadius, (float)(3*Math.PI/2), 0, true );
				strokePath.AddLineTo( new PointF( rect.Right, rect.Bottom ));
				strokePath.AddLineTo( new PointF( rect.Left, rect.Bottom ));
			}
			else if( _isBottom )
			{
				fillRectPath = UIBezierPath.FromRoundedRect(rect, UIRectCorner.BottomLeft | UIRectCorner.BottomRight, new SizeF(_cornerRadius,_cornerRadius) );
				strokePath.MoveTo( new PointF( rect.Right, rect.Top ));
				strokePath.AddLineTo( new PointF( rect.Right, rect.Bottom - _cornerRadius ));
				strokePath.AddArc( new PointF( rect.Right - _cornerRadius, rect.Bottom - _cornerRadius), _cornerRadius, 0, (float)(Math.PI/2), true );
				strokePath.AddLineTo( new PointF( rect.Left + _cornerRadius, rect.Bottom ));
				strokePath.AddArc( new PointF( rect.Left + _cornerRadius, rect.Bottom - _cornerRadius), _cornerRadius, (float)(Math.PI/2), (float)Math.PI, true );
				strokePath.AddLineTo( new PointF( rect.Left, rect.Top) );
			}
			else
			{
				fillRectPath = UIBezierPath.FromRect( rect );
				strokePath.MoveTo( new PointF( rect.Left, rect.Top) );
				strokePath.AddLineTo( new PointF( rect.Left, rect.Bottom ));
				strokePath.AddLineTo( new PointF( rect.Right, rect.Bottom) );
				strokePath.AddLineTo( new PointF( rect.Right, rect.Top) );
			}

			//Fill
			if( IsAddNewCell && !((UITableViewCell)Superview).Highlighted )
			{
				UIImage.FromFile( "Assets/Cells/addNewBackground.png" ).Draw( rect, CGBlendMode.Normal, 1f );
			}
			else if( IsAddNewCell && ((UITableViewCell)Superview).Highlighted )
			{
				UIImage.FromFile( "Assets/Cells/addNewBackgroundSelected.png" ).Draw( rect, CGBlendMode.Normal, 1f );
			}
			else
			{
	  			var backgroundColor = ((UITableViewCell)Superview).Highlighted ? _selectedBackgroundColor : _backgroundColor;
				backgroundColor.SetFill();
				fillRectPath.LineWidth = _strokeSize;
				fillRectPath.Fill();
			}


			//Inner Shadow
		    var roundedRectangleBorderRect = fillRectPath.Bounds;
			roundedRectangleBorderRect.X -= _innerShadowSidesBlurRadius;
			roundedRectangleBorderRect.Width += 2*_innerShadowSidesBlurRadius;
			roundedRectangleBorderRect.Y -= _isTop ? _innerShadowTopBottomBlurRadius : 0;
			roundedRectangleBorderRect.Height += _isBottom ? 2*_innerShadowTopBottomBlurRadius : 0;

            roundedRectangleBorderRect = RectangleF.Union(roundedRectangleBorderRect, fillRectPath.Bounds);
			roundedRectangleBorderRect.X -= 1;
			roundedRectangleBorderRect.Width += 2;
			roundedRectangleBorderRect.Y -= 0;
			roundedRectangleBorderRect.Height += 0;

            var roundedRectangleNegativePath = UIBezierPath.FromRect(roundedRectangleBorderRect);
            roundedRectangleNegativePath.AppendPath(fillRectPath);
            roundedRectangleNegativePath.UsesEvenOddFillRule = true;

            context.SaveState();
            {
                var xOffset = (float)Math.Round(roundedRectangleBorderRect.Width);
				var yOffset = 0;
                context.SetShadowWithColor(
                    new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
                    _innerShadowTopBottomBlurRadius,
					_strokeColor.CGColor);

                fillRectPath.AddClip();
                var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(roundedRectangleBorderRect.Width), 0);
                roundedRectangleNegativePath.ApplyTransform(transform);
                UIColor.Gray.SetFill();
                roundedRectangleNegativePath.Fill();
            }
            context.RestoreState();




			//Stroke
			context.SaveState();
            strokePath.LineWidth = _strokeSize;
            _strokeColor.SetStroke();
            strokePath.AddClip();
            context.AddPath(strokePath.CGPath);
            context.StrokePath();
            context.RestoreState();     

		}
	}
}
