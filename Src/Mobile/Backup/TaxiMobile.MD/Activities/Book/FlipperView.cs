//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;

//namespace TaxiMobile.Activities.Book
//{
//    class FlipperView
//    {
//        //private ViewFlipper _flipper;
//        //protected override bool IsRouteDisplayed
//        //{
//        //    get { return false; }
//        //}




//        //private void TogglePickupDestination(bool selectPickup)
//        //{
//        //    var pickupButton = FindViewById<Button>(Resource.Id.PickupBtn);
//        //    var destinationButton = FindViewById<Button>(Resource.Id.DestinationBtn);
//        //    pickupButton.Selected = selectPickup;
//        //    destinationButton.Selected = !selectPickup;

//        //    var selectedViewIsPickup = _flipper.CurrentView == FindViewById(Resource.Id.pickupView);



//        //    if ((selectPickup) && (!selectedViewIsPickup))
//        //    {
//        //        _flipper.InAnimation = GetInFromLeftAnimation();
//        //        _flipper.OutAnimation = GetOutToRightAnimation();
//        //        _flipper.ShowPrevious();

//        //    }
//        //    else if ((!selectPickup) && (selectedViewIsPickup))
//        //    {
//        //        _flipper.InAnimation = GetInFromRightAnimation();
//        //        _flipper.OutAnimation = GetOutToLeftAnimation();
//        //        _flipper.ShowNext();

//        //    }

//        //    if (selectPickup)
//        //    {
//        //        _pickupController.Activate();
//        //    }
//        //    else
//        //    {
//        //        _destinationController.Activate();
//        //    }
//        //}


//        //private Animation GetInFromRightAnimation()
//        //{


//        //    Animation inFromRight = new TranslateAnimation((int)Dimension.RelativeToParent, +1.0f, (int)Dimension.RelativeToParent, 0.0f, (int)Dimension.RelativeToParent, 0.0f, (int)Dimension.RelativeToParent, 0.0f);
//        //    inFromRight.Duration = 300;
//        //    inFromRight.Interpolator = new AccelerateInterpolator();
//        //    return inFromRight;
//        //}
//        //private Animation GetOutToLeftAnimation()
//        //{
//        //    Animation outtoLeft = new TranslateAnimation((int)Dimension.RelativeToParent, 0.0f, (int)Dimension.RelativeToParent, -1.0f, (int)Dimension.RelativeToParent, 0.0f, (int)Dimension.RelativeToParent, 0.0f);
//        //    outtoLeft.Duration = 300;
//        //    outtoLeft.Interpolator = new AccelerateInterpolator();
//        //    return outtoLeft;
//        //}

//        //private Animation GetInFromLeftAnimation()
//        //{
//        //    Animation inFromLeft = new TranslateAnimation((int)Dimension.RelativeToParent, -1.0f, (int)Dimension.RelativeToParent, 0.0f, (int)Dimension.RelativeToParent, 0.0f, (int)Dimension.RelativeToParent, 0.0f);
//        //    inFromLeft.Duration = 300;
//        //    inFromLeft.Interpolator = new AccelerateInterpolator();
//        //    return inFromLeft;
//        //}
//        //private Animation GetOutToRightAnimation()
//        //{
//        //    Animation outtoRight = new TranslateAnimation((int)Dimension.RelativeToParent, 0.0f, (int)Dimension.RelativeToParent, +1.0f, (int)Dimension.RelativeToParent, 0.0f, (int)Dimension.RelativeToParent, 0.0f);
//        //    outtoRight.Duration = 300;
//        //    outtoRight.Interpolator = new AccelerateInterpolator();
//        //    return outtoRight;
//        //}



//    }
//}