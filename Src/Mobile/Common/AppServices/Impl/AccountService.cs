using System;
using System.Linq;
using System.Collections.Generic;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using SocialNetworks.Services;

#if IOS
using ServiceStack.ServiceClient.Web;
using ServiceStack.Common.ServiceClient.Web;
#endif
using ServiceStack.Common.ServiceClient.Web;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Net;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class AccountService : BaseService, IAccountService
    {

        private const string _favoriteAddressesCacheKey = "Account.FavoriteAddresses";
        private const string _historyAddressesCacheKey = "Account.HistoryAddresses";
        private static ReferenceData _refData;

        public void EnsureListLoaded()
        {
            if ((_refData == null) || (_refData.CompaniesList.Count() == 0))
            {
                UseServiceClient<ReferenceDataServiceClient>(service =>
                {
                    _refData = service.GetReferenceData();
                }
                );
            }
        }

        protected ILogger Logger
        {
            get { return TinyIoCContainer.Current.Resolve<ILogger>(); }
        }

        public void ResendConfirmationEmail(string email)
        {

        }

        public void ClearCache ()
        {
            var serverUrl = TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl;
            _refData = null;
            TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_historyAddressesCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_favoriteAddressesCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService>().Clear("SessionId");
            TinyIoCContainer.Current.Resolve<ICacheService>().ClearAll();
            TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl = serverUrl; 
        }

        public void SignOut()
        {


            
        
            try
            {
                var facebook = TinyIoCContainer.Current.Resolve<IFacebookService>();
                if (facebook.IsConnected)
                {
                    facebook.SetCurrentContext(this);
                    facebook.Disconnect();
                }
            }
            catch
            {
            }
            try
            {
                var twitterService = TinyIoCContainer.Current.Resolve<ITwitterService>();
                if (twitterService.IsConnected)
                {
                    twitterService.Disconnect();
                }
            }
            catch
            {
            }

             ClearCache ();




            var dispatch = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher;
            dispatch.RequestNavigate(new MvxShowViewModelRequest(typeof(LoginViewModel), null, false, MvxRequestedBy.UserAction));
        }

        public void RefreshCache(bool reload)
        {
            TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_historyAddressesCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_favoriteAddressesCacheKey);

            if (reload)
            {
                GetFavoriteAddresses();
                GetHistoryAddresses();
            }
        }

        public IEnumerable<Address> GetHistoryAddresses()
        {
            var cached = TinyIoCContainer.Current.Resolve<ICacheService>().Get<Address[]>(_historyAddressesCacheKey);

            if (cached != null)
            {
                return cached;
            }
            else
            {

                IEnumerable<Address> result = new Address[0];
                UseServiceClient<AccountServiceClient>(service =>
                {
                    result = service.GetHistoryAddresses(CurrentAccount.Id);
                }
                );

                TinyIoCContainer.Current.Resolve<ICacheService>().Set(_historyAddressesCacheKey, result.ToArray());
                return result;
            }
        }

        public IEnumerable<Order> GetHistoryOrders()
        {
            IEnumerable<Order> result = new Order[0];
            UseServiceClient<OrderServiceClient>(service =>
            {
                result = service.GetOrders();
            }
            );

            return result;
        }

        public Order GetHistoryOrder(Guid id)
        {
			var result = default(Order);
			UseServiceClient<OrderServiceClient>(service =>
			                                     {
				result = service.GetOrder(id);
			}
			);
			return result;
        }

        public IEnumerable<Address> GetFavoriteAddresses()
        {
            var cached = TinyIoCContainer.Current.Resolve<ICacheService>().Get<Address[]>(_favoriteAddressesCacheKey);

            if (cached != null)
            {
                return cached;
            }
            else
            {

                IEnumerable<Address> result = new Address[0];
                UseServiceClient<AccountServiceClient>(service =>
                {
                    result = service.GetFavoriteAddresses();
                }
                );
                TinyIoCContainer.Current.Resolve<ICacheService>().Set(_favoriteAddressesCacheKey, result.ToArray());
                return result;
            }
        }

        private void UpdateCacheArray<T>(string key, T updated, Func<T, T, bool> compare)
        {
            var cached = TinyIoCContainer.Current.Resolve<ICacheService>().Get<T[]>(key);

            if (cached != null)
            {

                var found = cached.SingleOrDefault(c => compare(updated, c));
                if (found == null)
                {
                    T[] newList = new T[cached.Length + 1];
                    Array.Copy(cached, newList, cached.Length);
                    newList[cached.Length] = updated;

                    TinyIoCContainer.Current.Resolve<ICacheService>().Set(key, newList);
                }
                else
                {
                    var foundIndex = cached.IndexOf(updated, compare);
                    cached[foundIndex] = updated;
                    TinyIoCContainer.Current.Resolve<ICacheService>().Set(key, cached);
                }
            }


        }

        private void RemoveFromCacheArray<T>(string key, Guid toDeleteId, Func<Guid, T, bool> compare)
        {
            var cached = TinyIoCContainer.Current.Resolve<ICacheService>().Get<T[]>(key);

            if ((cached != null) && (cached.Length > 0))
            {
                var list = new List<T>(cached);
                var toDelete = list.Single(item => compare(toDeleteId, item));
                list.Remove(toDelete);
                TinyIoCContainer.Current.Resolve<ICacheService>().Set(key, list.ToArray());
            }


        }

        public Address FindInAccountAddresses(double latitude, double longitude)
        {
            Address found = null;

            UseServiceClient<AccountServiceClient>(service =>
            {

                var favorites = GetFavoriteAddresses();

                if (favorites.Count() > 0)
                {
                    found = favorites.FirstOrDefault(a => (Math.Abs(a.Longitude - longitude) <= 0.002) && (Math.Abs(a.Latitude - latitude) <= 0.002));
                }

            }
            );
            if (found == null)
            {
                var historics = GetHistoryAddresses();

                if (historics.Count() > 0)
                {
                    found = historics.FirstOrDefault(a => (Math.Abs(a.Longitude - longitude) <= 0.001) && (Math.Abs(a.Latitude - latitude) <= 0.001));
                }
            }

            return found;


        }

        public Account CurrentAccount
        {
            get
            {

                var account = TinyIoCContainer.Current.Resolve<ICacheService>().Get<Account>("LoggedUser");
                return account;
            }
            private set
            {
                if ( value != null )
                {
                    TinyIoCContainer.Current.Resolve<ICacheService>().Set("LoggedUser", value );    
                }
                else
                {
                    TinyIoCContainer.Current.Resolve<ICacheService>().Clear("LoggedUser");
                }                
            }
        }

        public bool CheckSession()
        {
            try
            {
                var client = TinyIoCContainer.Current.Resolve<AuthServiceClient>();
                client.CheckSession();

                return true;

            }
            catch
            {
                return false;
            }
        }

        public void UpdateSettings(BookingSettings settings)
        {
            QueueCommand<AccountServiceClient>(service =>
            {                     
                service.UpdateBookingSettings(new BookingSettingsRequest{ Name =  settings.Name, Phone=settings.Phone, Passengers = settings.Passengers, VehicleTypeId = settings.VehicleTypeId, ChargeTypeId = settings.ChargeTypeId, ProviderId = settings.ProviderId });
                CurrentAccount.Settings = settings;
                //Set to update the cache
                CurrentAccount = CurrentAccount;
            }
            );

        }

        public string UpdatePassword(Guid accountId, string currentPassword, string newPassword)
        {
            string response = null;
            QueueCommand<AccountServiceClient>(service => {                     
                response = service.UpdatePassword(new UpdatePassword() { AccountId = accountId, CurrentPassword = currentPassword, NewPassword = newPassword });
            });

            return response;
        }

        public Account GetAccount(string email, string password)
        {
            try
            {
                var auth = TinyIoCContainer.Current.Resolve<AuthServiceClient>();
                var authResponse = auth.Authenticate(email, password);
                SaveCredentials(authResponse);                
                return GetAccount( true);
            }
            catch (WebException ex)
            {
                TinyIoC.TinyIoCContainer.Current.Resolve<IErrorHandler>().HandleError(ex);
                
                return null;
            }
            catch (Exception e)
            {
                var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("InvalidLoginMessageTitle");
                var message = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("InvalidLoginMessage");

                TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, message);                

                
                return null;
            }
            
            
        }

        private static void SaveCredentials(AuthResponse authResponse)
        {
            var cache = TinyIoC.TinyIoCContainer.Current.Resolve<ICacheService>();
            cache.Set("SessionId", authResponse.SessionId);
        }

        public Account GetFacebookAccount(string facebookId)
        {
            try
            {
                var auth = TinyIoCContainer.Current.Resolve<AuthServiceClient>();
                var authResponse = auth.AuthenticateFacebook(facebookId);
                SaveCredentials(authResponse);
                return GetAccount(false);
            }
            catch
            {
                return null;
            }
        }

        public Account GetTwitterAccount(string twitterId)
        {
            try
            {
            var parameters = new NamedParameterOverloads();
            var auth = TinyIoCContainer.Current.Resolve<AuthServiceClient>();
            var authResponse = auth.AuthenticateTwitter(twitterId);
            SaveCredentials(authResponse);

            parameters.Add("credential", authResponse);
            return GetAccount( false);
            }
            catch
            {
                return null;
            }
        }

        private Account GetAccount(bool showInvalidMessage)
        {
            Account data = null;

            try
            {
                TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_historyAddressesCacheKey);
                TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_favoriteAddressesCacheKey);

                var service = TinyIoCContainer.Current.Resolve<AccountServiceClient>("Authenticate");
                var account = service.GetMyAccount();
                if (account != null)
                {
                    CurrentAccount = account;
                    data = account;
                }
                EnsureListLoaded();
            }
            catch (WebException ex)
            {
                TinyIoC.TinyIoCContainer.Current.Resolve<IErrorHandler>().HandleError(ex);
                return null;
            }
            catch (Exception e)
            {
                if (showInvalidMessage)
                {
                    var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("InvalidLoginMessageTitle");
                    var message = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("InvalidLoginMessage");
                    TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, message);
                }

                return null;
            }

            return data;
        }

        public bool ResetPassword(string email)
        {
            bool isSuccess = false;

            UseServiceClient<AccountServiceClient>("NotAuthenticated", service => {               
                service.ResetPassword(email);
                isSuccess = true;
            });


            return isSuccess;
        }

        public bool Register(RegisterAccount data, out string error)
        {

            bool isSuccess = false;

            string lError = "";

            data.AccountId = Guid.NewGuid();
            data.Language = TinyIoCContainer.Current.Resolve<IAppResource>().CurrentLanguageCode;

            try
            {
                lError = UseServiceClient<AccountServiceClient>(service =>
                {
                    service.RegisterAccount(data);
                    isSuccess = true;
                }
                );                
            }
            catch (Exception ex)
            {
                lError = ex.Message;
                isSuccess = false;
            }

            error = lError;
            return isSuccess;
        }

        public void DeleteFavoriteAddress(Guid addressId)
        {
            if (addressId.HasValue())
            {
                var toDelete = addressId;
                
                RemoveFromCacheArray<Address>(_favoriteAddressesCacheKey, toDelete, (id, a) => a.Id == id);                

                QueueCommand<AccountServiceClient>(service => service.RemoveFavoriteAddress(toDelete));
            }
        }

        public void DeleteHistoryAddress(Guid addressId)
        {
            if (addressId.HasValue())
            {
                var toDelete = addressId;

                RemoveFromCacheArray<Address>(_historyAddressesCacheKey, toDelete, (id, a) => a.Id == id);

                QueueCommand<AccountServiceClient>(service => service.RemoveAddress(toDelete));
            }
        }

        public void UpdateBookingSettings(BookingSettings bookingSettings)
        {
            BookingSettingsRequest bsr = new BookingSettingsRequest()
                                             {
                                                 ChargeTypeId = bookingSettings.ChargeTypeId,
                                                 Name = bookingSettings.Name,
                                                 NumberOfTaxi = bookingSettings.NumberOfTaxi,
                                                 Passengers = bookingSettings.Passengers,
                                                 Phone = bookingSettings.Phone,
                                                 ProviderId = bookingSettings.ProviderId,
                                                 VehicleTypeId = bookingSettings.VehicleTypeId
                                             };
            QueueCommand<AccountServiceClient>(service =>
            {
                service.UpdateBookingSettings(bsr);
            });
        }

        public void UpdateAddress(Address address)
        {
            bool isNew = address.Id.IsNullOrEmpty();
            if (isNew)
            {
                
                address.Id = Guid.NewGuid();
            }


            address.IsHistoric = false;

            UpdateCacheArray(_favoriteAddressesCacheKey, address, (a1, a2) => a1.Id.Equals(a2.Id));


            QueueCommand<AccountServiceClient>(service =>
            {
                var toSave = new SaveAddress
                    {
                        Apartment = address.Apartment,
                        FriendlyName = address.FriendlyName,
                        FullAddress = address.FullAddress,
                        Id = address.Id,
                        Latitude = address.Latitude,
                        Longitude = address.Longitude,
                        RingCode = address.RingCode
                    };

                var toMove = toSave;

                if (isNew)
                {                        
                    service.AddFavoriteAddress(toSave);
                }
                else if (address.IsHistoric)
                {
                    service.UpdateFavoriteAddress(toMove);
                }
                else
                {
                    service.UpdateFavoriteAddress(toSave);
                }

            }
            );



        }

        public IEnumerable<ListItem> GetCompaniesList()
        {
            EnsureListLoaded();
            return _refData.CompaniesList;
        }

        public IEnumerable<ListItem> GetVehiclesList()
        {
            EnsureListLoaded();
            return _refData.VehiclesList;
        }

        public IEnumerable<ListItem> GetPaymentsList()
        {
            EnsureListLoaded();
            return _refData.PaymentsList;
        }
    }
}


