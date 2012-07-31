using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.GoogleMaps;
using Android.Graphics.Drawables;
using Android.Locations;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Activities.Location;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.Client.Converters;
using TinyIoC;
using WS = apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    public abstract class AddressActivity : MapActivity
    {
        protected GeoPoint _lastCenter;
        private bool _updateReceived;
        private bool _isInit = false;
        protected override bool IsRouteDisplayed
        {
            get { return false; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            _updateReceived = false;
        }

        protected void InitializeDropDownMenu()
        {
            //Initialize dropdown control

            var contactIntent = new Intent(Intent.ActionPick, ContactsContract.CommonDataKinds.StructuredPostal.ContentUri);
            var iconActionControl = new IconActionControl(this, "images/arrow-right@2x.png", new List<IconAction>() { new IconAction("images/favorite-icon@2x.png", contactIntent, 42) }, true);
            var dropDownControlLayout = FindViewById<LinearLayout>(Resource.Id.linear_iconaction);
            dropDownControlLayout.AddView(iconActionControl);
        }

        void HandleItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            OnAddressChanged(Address.Text, true);
        }

        protected BookActivity ParentActivity
        {
            get { return (BookActivity)Parent; }
        }

        void Address_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if ((e.ActionId == ImeAction.Search) || (e.ActionId == ImeAction.Done))
            {
                OnAddressChanged(Address.Text, false);
            }

        }

        protected override void OnResume()
        {
            base.OnResume();


            InitMap();

            MapService.AddMyLocationOverlay(Map, this);
            Address.EditorAction -= new EventHandler<TextView.EditorActionEventArgs>(Address_EditorAction);
            Address.EditorAction += new EventHandler<TextView.EditorActionEventArgs>(Address_EditorAction);
            Address.ItemClick -= HandleItemClick;
            Address.ItemClick += HandleItemClick;

            Address.FocusChange -= new EventHandler<View.FocusChangeEventArgs>(Address_FocusChange);
            Address.FocusChange += new EventHandler<View.FocusChangeEventArgs>(Address_FocusChange);

            //SelectAddressButton.Click -= SelectAddressButtonOnClick;
            //SelectAddressButton.Click += SelectAddressButtonOnClick;

            HideKeyboards();

        }



        private void SelectAddressButtonOnClick(object sender, EventArgs eventArgs)
        {

            Intent i = new Intent(this, typeof(LocationsActivity));
            i.PutExtra(NavigationStrings.ParentScreen.ToString(), (int)ParentScreens.BookScreen);
            Parent.StartActivityForResult(i, (int)ActivityEnum.Pickup);

        }


        protected override void OnPause()
        {
            base.OnPause();
            HideKeyboards();
        }


        void Address_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                SetAutoComplete();
            }
            else
            {               
                ClearAutoComplete();
                ValidateAddress(true);
            }
        }


        protected abstract MapView Map
        {
            get;
        }


        protected abstract int TitleResourceId
        {
            get;
        }

        protected abstract WS.Address Location
        {
            get;
            set;
        }

        protected abstract bool NeedFindCurrentLocation
        {
            get;
        }

        protected abstract AutoCompleteTextView Address
        {
            get;
        }

        protected abstract Button SelectAddressButton { get; }

        public virtual void OnAddressChanged(string address, bool useFirst)
        {
            if (address.IsNullOrEmpty())
            {
                if (NeedFindCurrentLocation)
                {
                    RetrieveCurrentLocation(UpdateLocationFromAndroidPosition);
                }
            }
            else
            {
                SearchForAddress(address, useFirst, true);
            }

        }

        public void ValidateAddress(bool executeInThread)
        {
            RunOnUiThread(() => Address.Error = null);

            Action validate = () =>
            {
                
                var found = TinyIoCContainer.Current.Resolve<IGeolocService>().ValidateAddress(Address.Text);
                if (found != null)
                {                
                    SetLocationData(found, true);
                }
                else
                {                    
                    ClearLocationLngLAt();
                }
            };

            if (executeInThread)
            {
                ThreadHelper.ExecuteInThread(Parent, validate, false);
            }
            else
            {
                validate();
            }
        }

        private void ClearLocationLngLAt()
        {
            RunOnUiThread(() => Address.Error = GetString ( Resource.String.InvalidAddressTextEdit ));
            Location.Latitude = 0;
            Location.Longitude = 0;
        }


        private void SearchForAddress(string address, bool useFirst, bool changeZoom)
        {
            RunOnUiThread(() => Address.Error = null);

            ThreadHelper.ExecuteInThread(Parent, () =>
                {
                    
                    var found = TinyIoCContainer.Current.Resolve<IGeolocService>().ValidateAddress(address);

                    if (found != null)
                    {                        
                        SetLocationData(found, changeZoom);                        
                    }
                    else
                    {                        
                        ClearLocationLngLAt();
                    }
                }, false);
        }
        private void SearchForAddress(GeoPoint point, bool useFirst, bool changeZoom)
        {

            ThreadHelper.ExecuteInThread(Parent, () =>
            {

                var addresses = TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(CoordinatesConverter.ConvertFromE6(Map.MapCenter.LatitudeE6), CoordinatesConverter.ConvertFromE6(Map.MapCenter.LongitudeE6));


                if ((addresses.Count() == 1) || (useFirst && addresses.Any()))
                {
                    var location = addresses.First();
                    SetLocationData(location, changeZoom);
                    location.ToSafeString();
                }
            }, false);
        }



        public virtual void SetLocationData(WS.Address location, bool changeZoom)
        {
            RunOnUiThread(() =>
                              {
                                  Address.FocusChange -= new EventHandler<View.FocusChangeEventArgs>(Address_FocusChange);

                                  try
                                  {
                                      Address.Text = location.FullAddress;

                                      if (location.HasValidCoordinate())
                                      {
                                          _lastCenter = MapService.SetLocationOnMap(Map, location);
                                          MapService.AddPushPin(Map, MapPin, location, this, GetString(TitleResourceId));
                                          MapService.SetLocationOnMap(Map, location);
                                          if (changeZoom)
                                          {
                                              Map.Controller.SetZoom(17);
                                          }
                                      }
                                      Address.ClearFocus();
                                      HideKeyboards();
                                  }
                                  catch(Exception ex)
                                  {
                                      TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                                  }
                                  Address.FocusChange += new EventHandler<View.FocusChangeEventArgs>(Address_FocusChange);
                              });


            Location = location;
        }

        protected void HideKeyboards()
        {
            RunOnUiThread(() =>
                              {
                                  var manager = (InputMethodManager)GetSystemService(Context.InputMethodService);

                                  foreach (var view in GetHideableControls())
                                  {
                                      manager.HideSoftInputFromWindow(view.WindowToken, 0);
                                  }
                              });

        }

        protected virtual View[] GetHideableControls()
        {
            return new View[] { Address };
        }


        private void UpdateLocationFromAndroidPosition(Android.Locations.Location location, bool timeoutExpired, bool changeZoom)
        {
            try
            {

                var address = TinyIoCContainer.Current.Resolve<IAccountService>().FindInAccountAddresses(location.Latitude, location.Longitude);
                if (address == null)
                {
                    address = TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(location.Latitude, location.Longitude).FirstOrDefault();
                }

                SetLocationData(address, changeZoom);
            }
            catch
            {
            }
        }






        public void ParentResume()
        {
            InitMap();
        }


        #region Map Manipulation
        protected void InitMap()
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;
            Map.SetBuiltInZoomControls(true);
            Map.Clickable = true;
            Map.Traffic = false;
            Map.Satellite = false;
            Map.Controller.SetZoom(15);

            if (Map is TouchMap)
            {
                ((TouchMap)Map).MapTouchUp -= new EventHandler(AddressActivity_MapTouchUp);
                ((TouchMap)Map).MapTouchUp += new EventHandler(AddressActivity_MapTouchUp);
            }

            var loc = new WS.Address { Longitude = -73.6162, Latitude = 45.54015 };
            if (ParentActivity.LocationService.LastLocation != null)
            {
                loc = new WS.Address { Longitude = ParentActivity.LocationService.LastLocation.Longitude, Latitude = ParentActivity.LocationService.LastLocation.Latitude };
            }

            //_lastCenter = MapService.SetLocationOnMap(Map, loc);
        }





        private void RetrieveCurrentLocation(Action<Android.Locations.Location, bool, bool> callback)
        {
            bool canceled = false;
            Action cancelAction= ()=> { 
                                        _updateReceived = true;
                                          canceled = true;
                                      };
            var progressDialog = new ProgressDialog(Parent);
            progressDialog.SetMessage(Resources.GetString(Resource.String.Locating));
            progressDialog.SetButton(Resources.GetString(Resource.String.CancelBoutton), (e,s)=> cancelAction());
            progressDialog.CancelEvent += (e, s) => cancelAction();
            progressDialog.Show();


            ThreadHelper.ExecuteInThread(Parent, () =>
                                                     {

                                                         try
                                                         {

                                                             bool timeoutExpired = false;
                                                             var location =
                                                                 ParentActivity.LocationService.WaitForAccurateLocation(
                                                                    6000, 200, out timeoutExpired);

                                                             if (!canceled)
                                                             {
                                                                 RunInUIThreadAndWait(() => callback(location, timeoutExpired, true));
                                                             }
                                                         }
                                                         finally
                                                         {
                                                             progressDialog.Maybe(() => progressDialog.Dismiss());
                                                         }

                                                     }, false);
        }




        public void RunInUIThreadAndWait(Action action)
        {

            var autoReset = new AutoResetEvent(false);

            RunOnUiThread(() =>
            {
                action();
                autoReset.Set();
            });

            autoReset.WaitOne();




        }

        protected virtual bool ShowUserLocation
        {
            get
            {
                return false;
            }
        }


        protected abstract Drawable MapPin
        {
            get;
        }



        private void SetAutoComplete(string[] list = null)
        {
            if (list == null)
            {
                //TODO : Fix this
                list = TinyIoCContainer.Current.Resolve<IAccountService>().GetFavoriteAddresses().Select(a => a.FullAddress).ToArray();
            }
            Address.Adapter = new ArrayAdapter<string>(this, Resource.Layout.ListItemAutoComplete, list);
        }

        private void ClearAutoComplete()
        {
            Address.Adapter = new ArrayAdapter<string>(this, Resource.Layout.ListItemAutoComplete, new string[0]);
        }

        void AddressActivity_MapTouchUp(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {

                HideKeyboards();
                if (_lastCenter == null ||
                    !(_lastCenter.LatitudeE6 == Map.MapCenter.LatitudeE6 &&
                      _lastCenter.LongitudeE6 == Map.MapCenter.LongitudeE6))
                {
                    SearchForAddress(Map.MapCenter, true, false);
                }


            });
        }




        #endregion




    }



}