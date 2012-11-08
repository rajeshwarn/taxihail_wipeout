
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.ObjCRuntime;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class BookRatingCell : MvxBindableTableViewCell
	{
		public static NSString Identifier = new NSString("BookRatingCell");
		public const string BindingText = "{'RatingTypeName':{'Path':'RatingTypeName'}}";
		
		public static BookRatingCell LoadFromNib(NSObject owner)
		{
			// this bizarre loading sequence is modified from a blog post on AlexYork.net
			// basically we create an empty cell in C#, then pass that through a NIB loading, which then magically
			// gives us a new cell back in MonoTouch again
			var views = NSBundle.MainBundle.LoadNib("BookRatingCell", owner, null);
			var cell2 = Runtime.GetNSObject( views.ValueAt(0) ) as BookRatingCell;
			views = null;
			cell2.Initialise();
			return cell2;
		}
		
		public BookRatingCell(IntPtr handle)
			: base(BindingText, handle)
		{
		}		
		
		public BookRatingCell ()
			: base(BindingText, MonoTouch.UIKit.UITableViewCellStyle.Default, Identifier)
		{
		}
		
		public BookRatingCell (string bindingText)
			: base(bindingText, MonoTouch.UIKit.UITableViewCellStyle.Default, Identifier)
		{
		}
		
		private void Initialise()
		{

		}	
		
		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				// TODO - really not sure that Dispose is the right place for this call 
				// - but couldn't see how else to do this in a TableViewCell
				ReleaseDesignerOutlets();
			}
			
			base.Dispose (disposing);
		} 
		
		public override string ReuseIdentifier 
		{
			get 
			{
				return Identifier.ToString();
			}
		}

		public string RatingTypeName
		{
			get { return ratingTypeName.Text; }
			set { if (ratingTypeName != null) ratingTypeName.Text = value; }
		}


		

	}
}

