using System;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Interfaces.Commands;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	public partial class BookRatingCell : MvxBindableTableViewCell
	{
		public static NSString Identifier = new NSString("BookRatingCell");
		public const string BindingText = @"{'RatingTypeName':{'Path':'RatingTypeName'},
'SetRateCommand': {'Path': 'SetRateCommand'},
'CanRate': {'Path': 'CanRating'},
'MadSelected': {'Path': 'MadSelected'},
'UnhappySelected': {'Path': 'UnhappySelected'},
'NeutralSelected': {'Path': 'NeutralSelected'},
'HappySelected': {'Path': 'HappySelected'},
'EcstaticSelected': {'Path': 'EcstaticSelected'}}";
		
		public static BookRatingCell LoadFromNib(NSObject owner)
		{
			// this bizarre loading sequence is modified from a blog post on AlexYork.net
			// basically we create an empty cell in C#, then pass that through a NIB loading, which then magically
			// gives us a new cell back in MonoTouch again
			var views = NSBundle.MainBundle.LoadNib("BookRatingCell", owner, null);
			var cell2 = Runtime.GetNSObject( views.ValueAt(0) ) as BookRatingCell;
		    if (cell2 != null)
		    {
		        cell2.Initialise();
		        return cell2;
		    }
		    return null;
		}

		
		public BookRatingCell(IntPtr handle)
			: base(BindingText, handle)
		{
		}		
		
		public BookRatingCell ()
			: base(BindingText, UITableViewCellStyle.Default, Identifier)
		{
		}
		
		public BookRatingCell (string bindingText)
			: base(bindingText, UITableViewCellStyle.Default, Identifier)
		{
		}
		
		private void Initialise()
		{
			madBtn.SetImage(UIImage.FromBundle("Assets/Rating/mad"), UIControlState.Normal);
			madBtn.SetImage(UIImage.FromBundle("Assets/Rating/mad-selected"), UIControlState.Selected);
			madBtn.TouchUpInside += OnMadBtnTouchUpInside;

			unhappyBtn.SetImage(UIImage.FromBundle("Assets/Rating/unhappy"), UIControlState.Normal);
			unhappyBtn.SetImage(UIImage.FromBundle("Assets/Rating/unhappy-selected"), UIControlState.Selected);
			unhappyBtn.TouchUpInside += OnUnhappyBtnTouchUpInside;

			neutralBtn.SetImage(UIImage.FromBundle("Assets/Rating/neutral"), UIControlState.Normal);
			neutralBtn.SetImage(UIImage.FromBundle("Assets/Rating/neutral-selected"), UIControlState.Selected);
			neutralBtn.TouchUpInside += OnNeutralBtnTouchUpInside;

			happyBtn.SetImage(UIImage.FromBundle("Assets/Rating/happy"), UIControlState.Normal);
			happyBtn.SetImage(UIImage.FromBundle("Assets/Rating/happy-selected"), UIControlState.Selected);
			happyBtn.TouchUpInside += OnHappyBtnTouchUpInside;

			ecstaticBtn.SetImage(UIImage.FromBundle("Assets/Rating/ecstatic"), UIControlState.Normal);
			ecstaticBtn.SetImage(UIImage.FromBundle("Assets/Rating/ecstatic-selected"), UIControlState.Selected);
			ecstaticBtn.TouchUpInside += OnEcstasticBtnTouchUpInside;

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
	

		public IMvxCommand SetRateCommand {
			get ;
// ReSharper disable once UnusedAutoPropertyAccessor.Global
			set ;
		}

		private bool _canRate = true;
		public bool CanRate {
			get {
				return _canRate;
			}
			set {
				_canRate = value;
			}
		}

		private void OnMadBtnTouchUpInside (object sender, EventArgs args)
		{
			if (SetRateCommand != null) {
				SetRateCommand.Execute ("Mad");
			
			}
		}
		private void OnUnhappyBtnTouchUpInside (object sender, EventArgs args)
		{
			if (SetRateCommand != null) {
				SetRateCommand.Execute ("Unhappy");
			}
		}
		private void OnNeutralBtnTouchUpInside (object sender, EventArgs args)
		{
			if (SetRateCommand != null) {
				SetRateCommand.Execute ("Neutral");
			}
		}
		private void OnHappyBtnTouchUpInside (object sender, EventArgs args)
		{
			if (SetRateCommand != null) {
				SetRateCommand.Execute ("Happy");
			}
		}
		private void OnEcstasticBtnTouchUpInside (object sender, EventArgs args)
		{
			if (SetRateCommand != null) {
				SetRateCommand.Execute ("Ecstatic");
			}
		}

		public bool MadSelected {
			get{ return madBtn.Selected;}
			set{ if(madBtn != null) madBtn.Selected = value; }
		}

		public bool UnhappySelected {
			get{ return unhappyBtn.Selected;}
			set{ if(unhappyBtn != null) unhappyBtn.Selected = value; }
		}

		public bool NeutralSelected {
			get{ return neutralBtn.Selected;}
			set{ if(neutralBtn != null) neutralBtn.Selected = value; }
		}

		public bool HappySelected {
			get{ return happyBtn.Selected;}
			set{ if(happyBtn != null) happyBtn.Selected = value; }
		}

		public bool EcstaticSelected {
			get{ return ecstaticBtn.Selected;}
			set{ if(ecstaticBtn != null) ecstaticBtn.Selected = value; }
		}

	
	}
}

