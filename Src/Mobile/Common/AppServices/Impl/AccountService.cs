using System;
using System.Linq;
using System.Collections.Generic;
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
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class AccountService : BaseService, IAccountService
    {

        private const string _favoriteAddressesCacheKey = "Account.FavoriteAddresses";
        private const string _historyAddressesCacheKey = "Account.HistoryAddresses";
        private static ReferenceData _refData;

        public void EnsureListLoaded()
        {
            if (_refData == null)
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

        public void SignOut()
        {

            TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_historyAddressesCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_favoriteAddressesCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService>().Clear("SessionId");
            TinyIoCContainer.Current.Resolve<ICacheService>().ClearAll();
        
            try{
            TinyIoCContainer.Current.Resolve<ITwitterService>().Disconnect();
            }
            catch
            {
            }
            try{
            TinyIoCContainer.Current.Resolve<IFacebookService>().Disconnect();
            }
            catch
            {}

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
            return GetHistoryOrders().SingleOrDefault(o => o.Id == id);
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
                return TinyIoCContainer.Current.Resolve<IAppContext>().LoggedUser;
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
            catch (Exception e)
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
            }
            );

        }

        public Account GetAccount(string email, string password, out string error)
        {
            try
            {
                var parameters = new NamedParameterOverloads();
                var auth = TinyIoCContainer.Current.Resolve<AuthServiceClient>();
                var authResponse = auth.Authenticate(email, password);

                SaveCredentials(authResponse);

                parameters.Add("credential", authResponse);
                return GetAccount(parameters, out error);
            }
            catch (Exception e)
            {
                error = "Invalid login or password";
                return null;
            }
            
            
        }

        private static void SaveCredentials(AuthResponse authResponse)
        {
            var cache = TinyIoC.TinyIoCContainer.Current.Resolve<ICacheService>();
            cache.Set("SessionId", authResponse.SessionId);
        }

        public Account GetFacebookAccount(string facebookId, out string error)
        {
            try
            {
                var parameters = new NamedParameterOverloads();
                var auth = TinyIoCContainer.Current.Resolve<AuthServiceClient>();
                var authResponse = auth.AuthenticateFacebook(facebookId);
                SaveCredentials(authResponse);

                parameters.Add("credential", authResponse);
                return GetAccount(parameters, out error);
            }
            catch (Exception e)
            {
                error = "Invalid login or password";
                return null;
            }
        }

        public Account GetTwitterAccount(string twitterId, out string error)
        {
            try
            {
                var parameters = new NamedParameterOverloads();
                var auth = TinyIoCContainer.Current.Resolve<AuthServiceClient>();
                var authResponse = auth.AuthenticateTwitter(twitterId);
                SaveCredentials(authResponse);

                parameters.Add("credential", authResponse);
                return GetAccount(parameters, out error);
            }
            catch (Exception e)
            {
                error = "Invalid login or password";
                return null;
            }
        }

        private Account GetAccount(NamedParameterOverloads parameters, out string error)
        {
            error = "";
            string resultError = "";
            bool isSuccess = false;
            Account data = null;

            try
            {
                TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_historyAddressesCacheKey);
                TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_favoriteAddressesCacheKey);

                var context = TinyIoCContainer.Current.Resolve<IAppContext>();                                
                var service = TinyIoCContainer.Current.Resolve<AccountServiceClient>("Authenticate");
                var account = service.GetMyAccount();
                if (account != null)
                {
                    context.UpdateLoggedInUser(account, false);
                    data = account;

                }
                EnsureListLoaded();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("***************AUTH ERROR " + ex.Message);
                error = ex.Message;
                isSuccess = false;
            }
            return data;
        }

        public bool ResetPassword(string email)
        {
            bool isSuccess = false;

            try
            {
                var service = TinyIoCContainer.Current.Resolve<AccountServiceClient>("NotAuthenticated");
                service.ResetPassword(email);
                isSuccess = true;
            }
            catch (Exception ex)
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Error resetting the password");
                TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
            }

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
                //var service = TinyIoCContainer.Current.Resolve<AccountServiceClient>();
                //var service = new AccountServiceClient("http://192.168.12.125/apcurium.MK.Web/api/");
                //service.RegisterAccount(data);
                //isSuccess = true;
            }
            catch (Exception ex)
            {
                lError = ex.Message;
                isSuccess = false;
            }

            error = lError;
            return isSuccess;
        }

        public void DeleteAddress(Guid addressId)
        {
            if (addressId.HasValue())
            {
                var accountId = CurrentAccount.Id;
                var toDelete = addressId;
                
                RemoveFromCacheArray<Address>(_favoriteAddressesCacheKey, toDelete, (id, a) => a.Id == id);                

                QueueCommand<AccountServiceClient>(service =>
                {                     
                    service.RemoveFavoriteAddress(toDelete);
                }
                );
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
            }
            );

            
            
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

        public Account UpdateUser(Account data)
        {
            Account r = null;
            //UseService(service =>
            //{
            //    Logger.LogMessage("Update user");

            //    var sessionId = service.Authenticate("iphone", "test", 1);

            //    var account = service.GetAccount(sessionId, data.Email, data.Password);

            //    if (account.Error == IBS.ErrorCode.NoError)
            //    {
            //        Logger.LogMessage("Update user : No error");
            //        var toUpdate = new AccountMapping().ToWSData(account.Account, data);
            //        new SettingMapper().SetWSSetting(toUpdate, data);
            //        toUpdate.Password = data.Password;


            //        var result = service.UpdateAccount(sessionId, toUpdate);
            //        if (result.Error != IBS.ErrorCode.NoError)
            //        {
            //            r = data;
            //        }
            //        else
            //        {
            //            var loggedUser = TinyIoCContainer.Current.Resolve<IAppContext>().LoggedUser;
            //            r = new AccountMapping().ToData(loggedUser, result.Account);
            //        }
            //    }

            //    else
            //    {
            //        Logger.LogMessage("Update user : Error : " + account.Error.ToString() + " - " + account.ErrorMessage.ToSafeString());
            //    }


            //});
            return data;
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


