using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements;

namespace apcurium.MK.Booking.Mobile.Client
{

	public class RightAlignedMvvmCrossEntryElement: EntryElement
	{
		public RightAlignedMvvmCrossEntryElement (string caption, string placeholder, string value, bool isPassword)
			: base (caption, placeholder, value,isPassword)
		{
		}
		
		public RightAlignedMvvmCrossEntryElement (string caption, string placeholder)
			: base (caption, placeholder)
		{
		}

		protected override UITableViewCell GetCellImpl (UITableView tv)
		{
			var cell = base.GetCellImpl(tv);
			cell.BackgroundColor = UIColor.Clear;
			return cell;
		}	

		protected override UITextField CreateTextField (RectangleF frame)
		{
			var res = base.CreateTextField (new RectangleF(frame.X, frame.Y, frame.Width - 10, frame.Height ));
			res.TextAlignment = UITextAlignment.Right;
			return res;
		}
		
		NSString cellKey = new NSString ("RightAligned");
		protected override NSString CellKey { get { return cellKey; } }
	}
} 