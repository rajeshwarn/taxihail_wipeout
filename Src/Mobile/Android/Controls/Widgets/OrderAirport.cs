using System;
using Android.Content;
using Android.Util;
using Android.Widget;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Views;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Controls.Behavior;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class OrderAirport : MvxFrameControl
    {
        private TextView lblAirport;
        private TextView lblAirline;
        private TextView lblFlightNum;
        private TextView lblPUPoints;
        private TextView lblDateTime;

        private TextView txtDateTime;

        private EditTextSpinner txtAirlines;
        private EditTextSpinner txtPUPoints;
        private EditText txtFlightNum;
        private EditTextEntry txtEditNote;
        private LinearLayout _bottomPadding;

        public OrderAirport(Context context, IAttributeSet attrs) : base (LayoutHelper.GetLayoutForView(Resource.Layout.SubView_OrderAirport, context), context, attrs)
		{
            this.DelayBind(() => 
            {
                lblAirport = Content.FindViewById<TextView>(Resource.Id.lblAirport);
                lblAirline = Content.FindViewById<TextView>(Resource.Id.lblAirline);
                lblFlightNum = Content.FindViewById<TextView>(Resource.Id.lblFlightNum);
                lblPUPoints = Content.FindViewById<TextView>(Resource.Id.lblPUPoints);
                lblDateTime = Content.FindViewById<TextView>(Resource.Id.lblDateTime);

                txtPUPoints = Content.FindViewById<EditTextSpinner>(Resource.Id.txtPUPoints);
                txtAirlines = Content.FindViewById<EditTextSpinner>(Resource.Id.txtAirlines);
                txtFlightNum = Content.FindViewById<EditText>(Resource.Id.txtFlightNum);
                txtEditNote = FindViewById<EditTextEntry>(Resource.Id.txtAirportNotes);
                txtDateTime = Content.FindViewById<Button>(Resource.Id.txtDateTime);

                txtEditNote.SetClickAnywhereToDismiss();

                // hack for scroll in view when in EditText
                _bottomPadding = Content.FindViewById<LinearLayout>(Resource.Id.HackBottomPadding);
                TextFieldInHomeSubviewsBehavior.ApplyTo(
                    new List<EditText>() { txtEditNote, txtFlightNum },
                    () => _bottomPadding.Visibility = ViewStates.Visible,
                    () => _bottomPadding.Visibility = ViewStates.Gone
                );
                var hintTextColor = Resources.GetColor(Resource.Color.drivernode_hint_color);

                txtEditNote.SetHintTextColor(hintTextColor);
                txtDateTime.SetHintTextColor(hintTextColor);

                InitializeBinding();
            });
        }

        private OrderAirportViewModel ViewModel { get { return (OrderAirportViewModel)DataContext; } }

        void InitializeBinding()
		{
            var set = this.CreateBindingSet<OrderAirport, OrderAirportViewModel>();

            set.Bind(txtDateTime)
                .For(v => v.Text)
                .To(vm => vm.PickupTimeStamp);

            set.Bind(txtDateTime)
                .For("Click")
                .To(vm => vm.NavigateToDatePicker);

            set.Bind(lblAirport)
                .For(v => v.Text)
                .To(vm => vm.Title);

            set.Bind(txtFlightNum)
                .For(v => v.Text)
                .To(vm => vm.FlightNum );

            set.Bind(txtAirlines)
                .For("Text")
                .To(vm => vm.AirlineName);

            set.Bind(txtAirlines)
                .For("Data")
                .To(vm => vm.Airlines);

            set.Bind(txtAirlines)
                .For("SelectedItem")
                .To(vm => vm.AirlineId);

            set.Bind(txtEditNote)
                .For(v => v.Text)
                .To(vm => vm.Note);

            set.Bind(txtPUPoints)
                .For("Text")
                .To(vm => vm.PUPointsName);

            set.Bind(txtPUPoints)
                .For("Data")
                .To(vm => vm.PUPoints);

            set.Bind(txtPUPoints)
                .For("SelectedItem")
                .To(vm => vm.PUPointsId);
            
            set.Apply();
		}
    }
}
      

