using System;
using System.Drawing;
using MonoTouch.UIKit;

namespace TaxiMobileApp
{
	public class DestPickToggleButton
	{
		private UIButton _btnDest;
		private UIButton _btnPick;

		private Action _destAction;
		private Action _pickAction;

		public DestPickToggleButton (UIView pickView, UIView destView, Action pickAction, Action destAction)
		{
			_destAction = destAction;
			_pickAction = pickAction;
			
			
			pickView.BackgroundColor = UIColor.Clear;
			destView.BackgroundColor = UIColor.Clear;
			
			_btnDest = UIButton.FromType (UIButtonType.Custom);
			_btnDest.SetImage (UIImage.FromFile (Resources.DestinationOnButtonImageName), UIControlState.Normal);
			_btnDest.SetImage (UIImage.FromFile (Resources.DestinationOffButtonImageName), UIControlState.Selected);
			
			_btnDest.Frame = new RectangleF (0, 0, 100, 33);
			destView.AddSubview (_btnDest);
			
			_btnDest.TouchUpInside += HandleBtnDestTouchUpInside;
			
			_btnPick = UIButton.FromType (UIButtonType.Custom);
			_btnPick.SetImage (UIImage.FromFile (Resources.PickupOnButtonImageName), UIControlState.Normal);
			_btnPick.SetImage (UIImage.FromFile (Resources.PickupOffButtonImageName), UIControlState.Selected);
			
			
			
			_btnPick.Frame = new RectangleF (0, 0, 100, 33);
			
			_btnPick.TouchUpInside += HandleBtnPickTouchUpInside;
			pickView.AddSubview (_btnPick);
			
			SelectedButton = 0;
			
		}

		private int _selectedButton = 0;
		public int SelectedButton {
			get { return _selectedButton; }
			set {
				_selectedButton = value;
				if (_selectedButton == 0)
				{
					_btnDest.Selected = true;
					_btnPick.Selected = false;
					_btnDest.UserInteractionEnabled = true;
					_btnPick.UserInteractionEnabled = false;					
				}
				else
				{
					_btnDest.Selected = false;
					_btnPick.Selected = true;
					_btnDest.UserInteractionEnabled = false;
					_btnPick.UserInteractionEnabled = true;
				}
			}
		}
		void HandleBtnPickTouchUpInside (object sender, EventArgs e)
		{
			_btnPick.Selected = !_btnPick.Selected;
			_btnDest.Selected = !_btnPick.Selected;
			
			_btnPick.UserInteractionEnabled = _btnPick.Selected;
			_btnDest.UserInteractionEnabled = _btnDest.Selected;
			
			_pickAction ();
		}

		void HandleBtnDestTouchUpInside (object sender, EventArgs e)
		{
			_btnPick.Selected = !_btnPick.Selected;
			_btnDest.Selected = !_btnPick.Selected;
			
			_btnPick.UserInteractionEnabled = _btnPick.Selected;
			_btnDest.UserInteractionEnabled = _btnDest.Selected;
			
			_destAction ();
		}
	}
}

