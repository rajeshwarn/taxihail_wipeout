﻿using System;
using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Widget;
using ServiceStack.Text;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Activities.Setting;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using TinyMessenger;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait )]
    public class BookDetailActivity : BaseBindingActivity<BookConfirmationViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_BookingDetail; }
        }


        protected override void OnViewModelSet()
        {            

            SetContentView(Resource.Layout.View_BookingDetail);
            
            FindViewById<Button>(Resource.Id.EditBtn).Click += new EventHandler(Edit_Click);
            if (TinyIoCContainer.Current.Resolve<ICacheService>().Get<string>("WarningEstimateDontShow").IsNullOrEmpty() && ViewModel.Order.DropOffAddress.HasValidCoordinate())
            {
                ShowAlertDialog();
            }
            
            if (TinyIoCContainer.Current.Resolve<IAppSettings>().CanChooseProvider)
            {
				if(ViewModel.Order.Settings.ProviderId ==null)
                {
                    ShowChooseProviderDialog();
                }
            }
        }

        public void ShowChooseProviderDialog()
        {
            var service = TinyIoCContainer.Current.Resolve<IAccountService>();
            var companyList = service.GetCompaniesList();
            var hashmap = new List<int>();

            for(int i=0; i< companyList.Count();i++)
            {
                hashmap.Add(companyList.ElementAt(i).Id);
            }

            string[] list = companyList.Select(c => c.Display).ToArray();
            var chooseProviderDialog = new AlertDialog.Builder(this);
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SelectDialogItem, list);
            chooseProviderDialog.SetTitle("Pick a provider");
            chooseProviderDialog.SetAdapter(adapter, (sender, args) => 
            {
				ViewModel.Order.Settings.ProviderId = hashmap[(int)args.Which];
				TinyIoCContainer.Current.Resolve<IAccountService>().UpdateSettings(ViewModel.Order.Settings);
                FindViewById<TextView>(Resource.Id.CompanyTxt).Text = companyList.ElementAt(args.Which).Display;
                chooseProviderDialog.Dispose();
            });
            chooseProviderDialog.Show();
        }

        public void ShowAlertDialog()
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle(Resources.GetString(Resource.String.WarningEstimateTitle));
            alert.SetMessage(Resources.GetString(Resource.String.WarningEstimate));

            alert.SetPositiveButton("Ok", (s, e) => alert.Dispose());

            alert.SetNegativeButton(Resource.String.WarningEstimateDontShow, (s, e) =>
                {
                    TinyIoCContainer.Current.Resolve<ICacheService>().Set("WarningEstimateDontShow", "yes");
                    alert.Dispose();
                });

            alert.Show();
        }

        void Edit_Click(object sender, EventArgs e)
        {
            var i = new Intent().SetClass(this, typeof(RideSettingsActivity));
			i.PutExtra("BookingSettings", ViewModel.Order.Settings.Serialize());
            StartActivityForResult(i, 2);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if ((data != null) && (data.Extras != null))
            {
                var serializedSettings = data.GetStringExtra("BookingSettings");
                var bookingSettings = SerializerHelper.DeserializeObject<BookingSettings>(serializedSettings);
				ViewModel.Order.Settings = bookingSettings;
            }
        }
    }
}
