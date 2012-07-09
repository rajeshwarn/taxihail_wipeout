using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using TaxiMobile.Helpers;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Framework.Extensions;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services;
using TaxiMobile.Models;

namespace TaxiMobile.Activities.Location
{
    [Activity(Label = "Location Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = ScreenOrientation.Portrait)]
    public class LocationDetailActivity : Activity
    {
        private LocationData _data;
        public bool IsNew
        {
            get
            {
                return _data.IsNew;
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
            FindViewById<EditText>(Resource.Id.LocationAddress).Text = !_data.Address.IsNullOrEmpty() ? _data.Address : null;
            FindViewById<EditText>(Resource.Id.LocationAppartment).Text = !_data.Apartment.IsNullOrEmpty() ? _data.Apartment : null;
            FindViewById<EditText>(Resource.Id.RingCode).Text = !_data.RingCode.IsNullOrEmpty() ? _data.RingCode : null;
            FindViewById<EditText>(Resource.Id.LocationFriendlyName).Text = !_data.Name.IsNullOrEmpty() ? _data.Name : null;
            if (_data.IsFromHistory)
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

                    var locations = ServiceLocator.Current.GetInstance<IBookingService>().SearchAddress(txtAddress.Text);


                    if (locations.Count() != 1 || locations[0].Address.IsNullOrEmpty() || !locations[0].Longitude.HasValue || !locations[0].Latitude.HasValue)
                    {
                        return;
                    }


                    RunOnUiThread(() => { txtAddress.Text = locations[0].Address; });
                }, false);
            }
        }
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Finish();
        }
        private void SaveBtn_Click(object sender, EventArgs e)
        {
            var txtAddress = FindViewById<EditText>(Resource.Id.LocationAddress);
            if (txtAddress.Text.IsNullOrEmpty())
            {
                this.ShowAlert(Resource.String.InvalidAddressTitle, Resource.String.InvalidAddressMessage);
            }
            else
            {
                UpdateData();
                ThreadHelper.ExecuteInThread(this, () =>
            {
                try
                {
                    var locations = ServiceLocator.Current.GetInstance<IBookingService>().SearchAddress(txtAddress.Text);
                    if (locations.Count() != 1 || locations[0].Address.IsNullOrEmpty() || !locations[0].Longitude.HasValue || !locations[0].Latitude.HasValue)
                    {
                        RunOnUiThread(
                            () =>
                            this.ShowAlert(Resource.String.InvalidAddressTitle, Resource.String.InvalidAddressMessage));                        
                        return;
                    }

                    RunOnUiThread(() =>
                    {
                        txtAddress.Text = locations[0].Address;
                        UpdateData();
                        _data.Latitude = locations[0].Latitude;
                        _data.Longitude = locations[0].Longitude;
                        if (IsNew)
                        {
                            var newList = new List<LocationData>();
                            if ((AppContext.Current.LoggedUser.FavoriteLocations != null) && (AppContext.Current.LoggedUser.FavoriteLocations.Count() > 0))
                            {
                                newList.AddRange(AppContext.Current.LoggedUser.FavoriteLocations);
                            }
                            newList.Add(_data);
                            var loggedUser = AppContext.Current.LoggedUser;
                            loggedUser.FavoriteLocations = newList.ToArray();
                            AppContext.Current.UpdateLoggedInUser(loggedUser, true);
                            //AppContext.Current.LoggedUser = loggedUser;
                        }
                        else
                        {
                            if (AppContext.Current.LoggedUser.FavoriteLocations.Count() > 0  )
                            {
                                var list = new List<LocationData>(AppContext.Current.LoggedUser.FavoriteLocations);
                                var loc = list.FirstOrDefault(l => l.Id == _data.Id);
                                var i = list.IndexOf(loc);
                                list.RemoveAt(i);
                                list.Insert(i, _data);
                                AppContext.Current.LoggedUser.FavoriteLocations = list.ToArray();
                                //AppContext.Current.LoggedUser.FavoriteLocations.Remove(l => l.Id == _data.Id);
                                AppContext.Current.UpdateLoggedInUser(AppContext.Current.LoggedUser, true);
                            }
                        }
                        _data.IsFromHistory = false;
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
                var newList = new List<LocationData>();
                if ((AppContext.Current.LoggedUser.FavoriteLocations != null) &&
                    (AppContext.Current.LoggedUser.FavoriteLocations.Count() > 0))
                {
                    newList.AddRange(AppContext.Current.LoggedUser.FavoriteLocations);
                }
                newList.Remove(d => d.Id == _data.Id);
                AppContext.Current.LoggedUser.FavoriteLocations = newList.ToArray();
                AppContext.Current.UpdateLoggedInUser(AppContext.Current.LoggedUser, true);
                RunOnUiThread(() => Finish());
            }, true);



        }
        private void UpdateData()
        {
            _data.Address = FindViewById<EditText>(Resource.Id.LocationAddress).Text;
            _data.Apartment = FindViewById<EditText>(Resource.Id.LocationAppartment).Text;
            _data.RingCode = FindViewById<EditText>(Resource.Id.RingCode).Text;
            _data.Name = FindViewById<EditText>(Resource.Id.LocationFriendlyName).Text;
            _data.IsFromHistory = false;
        }
        private void SetLocationData(string serializedData)
        {


            LocationData data = SerializerHelper.DeserializeObject<LocationData>(serializedData);

            

            if ((data != null) && (data.Id > 0) && (!data.IsFromHistory))
            {
                _data = data;
            }
            else if ((data != null) && (data.Id > 0) && (data.IsFromHistory))
            {
                _data = data.Copy();
                _data.Id = 0;
                _data.IsNew = true;
            }
            else
            {
                _data = new LocationData();
                _data.IsNew = true;
            }



        }
    }
}