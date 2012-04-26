using System.Drawing;
using MonoTouch.UIKit;
using TaxiMobile.Localization;

namespace TaxiMobile.Book
{
	public class BookItButton
	{
		UIView _view;
		public BookItButton ( UIView view )
		{
			_view = view;
			
		}
		
		
		public UIButton Load()
		{
			var btnBookIt=UIButton.FromType( UIButtonType.Custom );			
			btnBookIt.SetImage( UIImage.FromFile(  Resources.BookItButtonImageName ), UIControlState.Normal);
		
			btnBookIt.Frame = new RectangleF( 0,0, 103, 45 );
			_view.AddSubview( btnBookIt );
			_view.BackgroundColor = UIColor.Clear;
			return btnBookIt;
		}
	}
}

