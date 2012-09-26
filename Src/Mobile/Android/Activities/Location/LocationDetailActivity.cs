using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Models;
using TinyIoC;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Location
{
    [Activity(Label = "Location Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LocationDetailActivity : Activity
    {
        private TinyMessageSubscriptionToken _closeViewToken;       
        private Address _data;
        public bool IsNew
        {
            get
            {
                return _data.Id.IsNullOrEmpty();
            }
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LocationDetail);
            SetLocationData(Intent.Extras.GetString(NavigationStrings.LocationSelectedId.ToString()));
            UpdateUI();
            _closeViewToken = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<CloseViewsToRoot>(m => Finish());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_closeViewToken != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<CloseViewsToRoot>(_closeViewToken);
            }
        }

        private void UpdateUI()
        {
            FindViewById<EditText>(Resource.Id.LocationAddress).Text = !_data.FullAddress.IsNullOrEmpty() ? _data.FullAddress : null;
            FindViewById<EditText>(Resource.Id.LocationAppartment).Text = !_data.Apartment.IsNullOrEmpty() ? _data.Apartment : null;
            FindViewById<EditText>(Resource.Id.RingCode).Text = !_data.RingCode.IsNullOrEmpty() ? _data.RingCode : null;
            FindViewById<EditText>(Resource.Id.LocationFriendlyName).Text = !_data.Id.IsNullOrEmpty() ? _data.FriendlyName : null;

            if (_data.Id.IsNullOrEmpty() )
            {
               FindViewById<Button>(Resource.Id.LocationDeleteBtn).Visibility = ViewStates.Gone;
               FindViewById<Button>(Resource.Id.LocationBookBtn).Visibility = ViewStates.Gone;
            }
            
            FindViewById<Button>(Resource.Id.LocationDeleteBtn).Click += new EventHandler(DeleteBtn_Click);            
            FindViewById<Button>(Resource.Id.LocationSaveBtn).Click += new EventHandler(SaveBtn_Click);
            FindViewById<Button>(Resource.Id.LocationBookBtn).Click += new EventHandler(BookBtn_Click);
            FindViewById<EditText>(Resource.Id.LocationAddress).FocusChange += new EventHandler<View.FocusChangeEventArgs>(LocationDetailActivity_FocusChange);

        }

        void LocationDetailActivity_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
            {
                var txtAddress = FindViewById<EditText>(Resource.Id.LocationAddress);
                ThreadHelper.ExecuteInThread(this, () =>
                {

                    var address = TinyIoCContainer.Current.Resolve<IGeolocService>().ValidateAddress(txtAddress.Text);                    

                    if (address == null )
                    {
                        return;
                    }


                    RunOnUiThread(() => { txtAddress.Text = address.FullAddress; });
                }, false);
            }
        }
     
        private bool ValidateFields()
        {
            var txtAddress = FindViewById<EditText>(Resource.Id.LocationAddress);
            var txtFriendlyName = FindViewById<EditText>(Resource.Id.LocationFriendlyName);
            if (txtAddress.Text.IsNullOrEmpty())
            {
                this.ShowAlert(Resource.String.InvalidAddressTitle, Resource.String.InvalidAddressMessage);
                return false;
            }
            if (txtFriendlyName.Text.IsNullOrEmpty())
            {
                this.ShowAlert(Resource.String.SaveAddressEmptyFieldTitle, Resource.String.SaveAddressEmptyFieldMessage);
                return false;
            }
            return true;

        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (ValidateFields())
            {
                var txtAddress = FindViewById<EditText>(Resource.Id.LocationAddress);
                UpdateData();
                ThreadHelper.ExecuteInThread(this, () =>
                {
                    try
                    {
                        var address = TinyIoCContainer.Current.Resolve<IGeolocService>().ValidateAddress(txtAddress.Text);
                        if ( address == null )
                        {
                            RunOnUiThread(
                                () =>
                                this.ShowAlert(Resource.String.InvalidAddressTitle, Resource.String.InvalidAddressMessage));                        
                            return;
                        }

                        RunOnUiThread(() =>
                        {
                            txtAddress.Text = address.FullAddress;
                            UpdateData();
                            _data.Latitude = address.Latitude;
                            _data.Longitude = address.Longitude;
                            TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().UpdateAddress(_data);
                            Finish();
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {

                    }
            }, true);
            }
            

        }



        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            ThreadHelper.ExecuteInThread(this, () =>
            {
                
                if ( _data.Id.HasValue() )
                {
                    if(_data.IsHistoric)
                    {
                        TinyIoCContainer.Current.Resolve<IAccountService>().DeleteHistoryAddress(_data.Id);
                    }
                    else
                    {
                        TinyIoCContainer.Current.Resolve<IAccountService>().DeleteFavoriteAddress(_data.Id);
                    }
                }

                RunOnUiThread(() => Finish());                
                
            }, true);
        }

        private void BookBtn_Click(object sender, EventArgs e)
        {
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish( new CloseViewsToRoot(this) );
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new BookUsingAddress(this, _data));            
        }

        private void UpdateData()
        {
            _data.FullAddress = FindViewById<EditText>(Resource.Id.LocationAddress).Text;
            _data.Apartment = FindViewById<EditText>(Resource.Id.LocationAppartment).Text;
            _data.RingCode = FindViewById<EditText>(Resource.Id.RingCode).Text;
            _data.FriendlyName = FindViewById<EditText>(Resource.Id.LocationFriendlyName).Text;
            //_data.IsFromHistory = false;
        }

        private void SetLocationData(string serializedData)
        {
            _data = SerializerHelper.DeserializeObject<Address>(serializedData);
            if (_data.FullAddress == GetString(Resource.String.LocationAddFavoriteSubtitle))
            {
                _data.FullAddress = "";
            }
        }
    }
}