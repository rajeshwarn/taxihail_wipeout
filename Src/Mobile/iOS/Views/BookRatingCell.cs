using System;
using System.Windows.Input;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using System.Collections.Generic;
using RatingState = apcurium.MK.Booking.Mobile.Models.RatingModel.RatingState;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	public partial class BookRatingCell : MvxStandardTableViewCell
	{
        public static float Height = 69f;
		public static NSString Identifier = new NSString("BookRatingCell");
		public const string BindingText = @"
            RatingTypeName RatingTypeName;
            SetRateCommand SetRateCommand;
            CanRate CanRate;
            ScoreASelected ScoreASelected;
            ScoreBSelected ScoreBSelected;
            ScoreCSelected ScoreCSelected;
            ScoreDSelected ScoreDSelected;
            ScoreESelected ScoreESelected;";
		
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
            var star = UIImage.FromBundle ("star_unselected");
            var selectedStar = UIImage.FromBundle ("star_selected");

            var buttons = new List<UIButton> () { btnScoreA, btnScoreB, btnScoreC, btnScoreD, btnScoreE };
            foreach (var button in buttons)
            {
                button.SetImage (star, UIControlState.Normal);
                button.SetImage (selectedStar, UIControlState.Selected);
            }

            btnScoreA.TouchUpInside += OnScoreATouchUpInside;
            btnScoreB.TouchUpInside += OnScoreBTouchUpInside;
            btnScoreC.TouchUpInside += OnScoreCTouchUpInside;
            btnScoreD.TouchUpInside += OnScoreDTouchUpInside;
            btnScoreE.TouchUpInside += OnScoreETouchUpInside;
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

		private void OnScoreATouchUpInside (object sender, EventArgs args)
		{
			if (SetRateCommand != null) {
                SetRateCommand.Execute (RatingState.ScoreA);
			}
		}

        private void OnScoreBTouchUpInside (object sender, EventArgs args)
		{
			if (SetRateCommand != null) {
                SetRateCommand.Execute (RatingState.ScoreB);
			}
		}

		private void OnScoreCTouchUpInside (object sender, EventArgs args)
		{
			if (SetRateCommand != null) {
                SetRateCommand.Execute (RatingState.ScoreC);
			}
		}

		private void OnScoreDTouchUpInside (object sender, EventArgs args)
		{
			if (SetRateCommand != null) {
                SetRateCommand.Execute (RatingState.ScoreD);
			}
		}

		private void OnScoreETouchUpInside (object sender, EventArgs args)
		{
			if (SetRateCommand != null) {
                SetRateCommand.Execute (RatingState.ScoreE);
			}
		}

		public bool ScoreASelected {
            get{ return btnScoreA.Selected;}
            set{ if(btnScoreA != null) btnScoreA.Selected = value; }
		}

		public bool ScoreBSelected {
            get{ return btnScoreB.Selected;}
            set{ if(btnScoreB != null) btnScoreB.Selected = value; }
		}

		public bool ScoreCSelected {
            get{ return btnScoreC.Selected;}
            set{ if(btnScoreC != null) btnScoreC.Selected = value; }
		}

		public bool ScoreDSelected {
            get{ return btnScoreD.Selected;}
            set{ if(btnScoreD != null) btnScoreD.Selected = value; }
		}

		public bool ScoreESelected {
            get{ return btnScoreE.Selected;}
            set{ if(btnScoreE != null) btnScoreE.Selected = value; }
		}
	}
}

