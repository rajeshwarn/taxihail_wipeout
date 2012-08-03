using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Models;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Location
{
    [Activity(Label = "Location Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LocationDetailActivity : Activity
    {
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
        }

        private void UpdateUI()
        {
            FindViewById<EditText>(Resource.Id.LocationAddress).Text = !_data.FullAddress.IsNullOrEmpty() ? _data.FullAddress : null;
            FindViewById<EditText>(Resource.Id.LocationAppartment).Text = !_data.Apartment.IsNullOrEmpty() ? _data.Apartment : null;
            FindViewById<EditText>(Resource.Id.RingCode).Text = !_data.RingCode.IsNullOrEmpty() ? _data.RingCode : null;
            FindViewById<EditText>(Resource.Id.LocationFriendlyName).Text = !_data.Id.IsNullOrEmpty() ? _data.FriendlyName : null;
            
            
            if (_data.Id.IsNullOrEmpty() )
            {
                FindViewById<Button>(Resource.Id.LocationDeleteBtn).Visibility = ViewStates.Invisible;
            }
            
            FindViewById<Button>(Resource.Id.LocationDeleteBtn).Click += new EventHandler(DeleteBtn_Click);
            FindViewById<Button>(Resource.Id.LocationCancelBtn).Click += new EventHandler(CancelBtn_Click);
            FindViewById<Button>(Resource.Id.LocationSaveBtn).Click += new EventHandler(SaveBtn_Click);
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
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Finish();
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

                        
                            //    //TODO: Fix this
                            //    //if ((AppContext.Current.LoggedUser.FavoriteLocations != null) && (AppContext.Current.LoggedUser.FavoriteLocations.Count() > 0))
                            //    //{
                            //    //    newList.AddRange(AppContext.Current.LoggedUser.FavoriteLocations);
                            //    //}
                            //    //newList.Add(_data);
                            //    //var loggedUser = AppContext.Current.LoggedUser;
                            
                            //    //TODO : Fix this
                            //    //loggedUser.FavoriteLocations = newList.ToArray();
                            
                            //    //AppContext.Current.UpdateLoggedInUser(loggedUser, true);
                            //    //AppContext.Current.LoggedUser = loggedUser;
                            //}
                            //else
                            //{
                            //    //TODO : Fix this
                            //    //if (AppContext.Current.LoggedUser.FavoriteLocations.Count() > 0  )
                            //    //{
                            //    //    var list = new List<LocationData>(AppContext.Current.LoggedUser.FavoriteLocations);
                            //    //    var loc = list.FirstOrDefault(l => l.Id == _data.Id);
                            //    //    var i = list.IndexOf(loc);
                            //    //    list.RemoveAt(i);
                            //    //    list.Insert(i, _data);
                            //    //    AppContext.Current.LoggedUser.FavoriteLocations = list.ToArray();
                            //    //    //AppContext.Current.LoggedUser.FavoriteLocations.Remove(l => l.Id == _data.Id);
                            //    //    AppContext.Current.UpdateLoggedInUser(AppContext.Current.LoggedUser, true);
                            //    //}
                            //}

                            //_data.IsFromHistory = false;
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
            TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().UpdateAddress(_data);

        }



        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            ThreadHelper.ExecuteInThread(this, () =>
            {

                if ( _data.Id.HasValue() )
                {
                    TinyIoCContainer.Current.Resolve<IAccountService>().DeleteAddress(_data.Id);
                }

                RunOnUiThread(() => Finish());

                //TODO : Fix this
                //var newList = new List<LocationData>();
                //if ((AppContext.Current.LoggedUser.FavoriteLocations != null) &&
                //    (AppContext.Current.LoggedUser.FavoriteLocations.Count() > 0))
                //{
                //    newList.AddRange(AppContext.Current.LoggedUser.FavoriteLocations);
                //}
                //newList.Remove(d => d.Id == _data.Id);
                //AppContext.Current.LoggedUser.FavoriteLocations = newList.ToArray();
                //AppContext.Current.UpdateLoggedInUser(AppContext.Current.LoggedUser, true);
                
            }, true);



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

            //TODO: Need to check if it's from history
            _data = SerializerHelper.DeserializeObject<Address>(serializedData);

            if (_data.FullAddress == GetString(Resource.String.LocationAddFavoriteSubtitle))
            {
                _data.FullAddress = "";
            }

            
            //if ((data != null) && (data.Id.
            //{
//                _data = data;
            //}
            //else if ((data != null) && (data.Id > 0) && (data.IsFromHistory))
            //{
            //    _data = data.Copy();
            //    _data.Id = Guid.Empty;                
            //}
            //else
            //{
            //    _data = new LocationData();                
            //}



        }
    }
}