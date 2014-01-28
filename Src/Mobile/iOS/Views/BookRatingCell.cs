using System;
using System.Windows.Input;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	public partial class BookRatingCell : MvxStandardTableViewCell
	{
		public static NSString Identifier = new NSString("BookRatingCell");
		public const string BindingText = @"
            RatingTypeName RatingTypeName;
            SetRateCommand SetRateCommand;
            CanRate CanRate;
            MadSelected MadSelected;
            UnhappySelected UnhappySelected;
            NeutralSelected NeutralSelected;
            HappySelected HappySelected;
            EcstaticSelected EcstaticSelected;";
		
		public static BookRatingCell LoadFromNib(NSObject owner)
		{
			// this bizarre loading sequence is modified from a blog post on AlexYork.net
			// basically we create an empty cell in C#, then pass that through a NIB loading, which then magically
			// gives us a new cell back in MonoTouch again
			var views = NSBundle.MainBundle.LoadNib("BookRatingCell", owner, null);
			var cell = Runtime.GetNSObject( views.ValueAt(0) ) as BookRatingCell;
		    if (cell != null)
		    {
		        cell.Initialise();
		        return cell;
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
			madBtn.SetImage(UIImage.FromFile("mad.png"), UIControlState.Normal);
			madBtn.SetImage(UIImage.FromFile("mad-selected.png"), UIControlState.Selected);
			madBtn.TouchUpInside += OnMadBtnTouchUpInside;

			unhappyBtn.SetImage(UIImage.FromFile("unhappy.png"), UIControlState.Normal);
			unhappyBtn.SetImage(UIImage.FromFile("unhappy-selected.png"), UIControlState.Selected);
			unhappyBtn.TouchUpInside += OnUnhappyBtnTouchUpInside;

			neutralBtn.SetImage(UIImage.FromFile("neutral.png"), UIControlState.Normal);
			neutralBtn.SetImage(UIImage.FromFile("neutral-selected.png"), UIControlState.Selected);
			neutralBtn.TouchUpInside += OnNeutralBtnTouchUpInside;

			happyBtn.SetImage(UIImage.FromFile("happy.png"), UIControlState.Normal);
			happyBtn.SetImage(UIImage.FromFile("happy-selected.png"), UIControlState.Selected);
			happyBtn.TouchUpInside += OnHappyBtnTouchUpInside;

			ecstaticBtn.SetImage(UIImage.FromFile("ecstatic.png"), UIControlState.Normal);
			ecstaticBtn.SetImage(UIImage.FromFile("ecstatic-selected.png"), UIControlState.Selected);
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
	
        public ICommand SetRateCommand { get; set; }

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

