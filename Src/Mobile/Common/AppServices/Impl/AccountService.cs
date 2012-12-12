using System;
using System.Linq;
using System.Collections.Generic;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using SocialNetworks.Services;
using apcurium.MK.Booking.Mobile.Data;

#if IOS
using ServiceStack.ServiceClient.Web;
using ServiceStack.Common.ServiceClient.Web;
#endif
using ServiceStack.Common.ServiceClient.Web;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Security;
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

        public void EnsureListLoaded ()
        {
            if ((_refData == null) || (_refData.CompaniesList.Count () == 0)) {
                UseServiceClient<ReferenceDataServiceClient> (service =>
                {
                    _refData = service.GetReferenceData ();
                }
                );
            }
        }

        protected ILogger Logger {
            get { return TinyIoCContainer.Current.Resolve<ILogger> (); }
        }

        public void ResendConfirmationEmail (string email)
        {

        }

        public void ClearCache ()
        {
            var serverUrl = TinyIoCContainer.Current.Resolve<IAppSettings> ().ServiceUrl;
            _refData = null;
            TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_historyAddressesCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_favoriteAddressesCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService> ().Clear ("AuthenticationData");
            TinyIoCContainer.Current.Resolve<ICacheService> ().ClearAll ();
            TinyIoCContainer.Current.Resolve<IAppSettings> ().ServiceUrl = serverUrl; 
        }

        public void SignOut ()
        {
            try {
                var facebook = TinyIoCContainer.Current.Resolve<IFacebookService> ();
                if (facebook.IsConnected) {
                    facebook.SetCurrentContext (this);
                    facebook.Disconnect ();
                }
            } catch {
            }
            try {
                var twitterService = TinyIoCContainer.Current.Resolve<ITwitterService> ();
                if (twitterService.IsConnected) {
                    twitterService.Disconnect ();
                }
            } catch {
            }

            ClearCache ();
          
        }

        public void RefreshCache (bool reload)
        {
            TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_historyAddressesCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_favoriteAddressesCacheKey);

            if (reload) {
                GetFavoriteAddresses ();
                GetHistoryAddresses ();
            }
        }

        public IEnumerable<Address> GetHistoryAddresses ()
        {
            var cached = TinyIoCContainer.Current.Resolve<ICacheService> ().Get<Address[]> (_historyAddressesCacheKey);
            if (cached != null) {
                return cached;
            } else {

                IEnumerable<Address> result = new Address[0];
                UseServiceClient<IAccountServiceClient> (service =>
                {
                    result = service.GetHistoryAddresses (CurrentAccount.Id);
                }
                );

                TinyIoCContainer.Current.Resolve<ICacheService> ().Set (_historyAddressesCacheKey, result.ToArray ());
                return result;
            }
        }

        public IEnumerable<Order> GetHistoryOrders ()
        {
            IEnumerable<Order> result = new Order[0];
            UseServiceClient<OrderServiceClient> (service =>
            {
                result = service.GetOrders ();
            }
            );

            return result;
        }

        public Order GetHistoryOrder (Guid id)
        {
            var result = default(Order);
            UseServiceClient<OrderServiceClient> (service =>
            {
                result = service.GetOrder (id);
            }
            );
            return result;
        }

        public IEnumerable<Address> GetFavoriteAddresses ()
        {
            var cached = TinyIoCContainer.Current.Resolve<ICacheService> ().Get<Address[]> (_favoriteAddressesCacheKey);

            if (cached != null) {
                return cached;
            } else {

                IEnumerable<Address> result = new Address[0];
                UseServiceClient<IAccountServiceClient> (service =>
                {
                    result = service.GetFavoriteAddresses ();
                }
                );
                TinyIoCContainer.Current.Resolve<ICacheService> ().Set (_favoriteAddressesCacheKey, result.ToArray ());
                return result;
            }
        }

        private void UpdateCacheArray<T> (string key, T updated, Func<T, T, bool> compare)
        {
            var cached = TinyIoCContainer.Current.Resolve<ICacheService> ().Get<T[]> (key);

            if (cached != null) {

                var found = cached.SingleOrDefault (c => compare (updated, c));
                if (found == null) {
                    T[] newList = new T[cached.Length + 1];
                    Array.Copy (cached, newList, cached.Length);
                    newList [cached.Length] = updated;

                    TinyIoCContainer.Current.Resolve<ICacheService> ().Set (key, newList);
                } else {
                    var foundIndex = cached.IndexOf (updated, compare);
                    cached [foundIndex] = updated;
                    TinyIoCContainer.Current.Resolve<ICacheService> ().Set (key, cached);
                }
            }


        }

        private bool RemoveFromCacheArray<T> (string key, Guid toDeleteId, Func<Guid, T, bool> compare)
        {
            var cached = TinyIoCContainer.Current.Resolve<ICacheService> ().Get<T[]> (key);

            if ((cached != null) && (cached.Length > 0)) {
                var list = new List<T> (cached);
                var toDelete = list.SingleOrDefault (item => compare (toDeleteId, item));
                var removed = list.Remove (toDelete);
                TinyIoCContainer.Current.Resolve<ICacheService> ().Set (key, list.ToArray ());
				return removed;
            }
			return false;
        }

        public Address FindInAccountAddresses (double latitude, double longitude)
        {
            Address found = GetAddresseInRange( GetFavoriteAddresses(), new apcurium.MK.Booking.Maps.Geo.Position( latitude, longitude ), 200);                   
            if (found == null) {
                found = GetAddresseInRange( GetHistoryAddresses(), new apcurium.MK.Booking.Maps.Geo.Position(  latitude,  longitude ), 100);
            }
            return found;

        }


        private Address GetAddresseInRange(IEnumerable<Address> addresses, apcurium.MK.Booking.Maps.Geo.Position position, float range)
        {             
            const double R = 6378137;
            
            var addressesInRange = from a in addresses
                let distance = position.DistanceTo(new apcurium.MK.Booking.Maps.Geo.Position(a.Latitude, a.Longitude))
                    where distance <= range
                    orderby distance ascending
                    select a;
            
            return addressesInRange.FirstOrDefault ();
        }

        
        public Account CurrentAccount {
            get {

                var account = TinyIoCContainer.Current.Resolve<ICacheService> ().Get<Account> ("LoggedUser");
                return account;
            }
            private set {
                if (value != null) {
                    TinyIoCContainer.Current.Resolve<ICacheService> ().Set ("LoggedUser", value);    
                } else {
                    TinyIoCContainer.Current.Resolve<ICacheService> ().Clear ("LoggedUser");
                }                
            }
        }

        public bool CheckSession ()
        {
            try {
                var client = TinyIoCContainer.Current.Resolve<IAuthServiceClient> ();
                client.CheckSession ();

                return true;

            } catch {
                return false;
            }
        }

        public void UpdateSettings (BookingSettings settings)
        {
            var bsr = new BookingSettingsRequest
            {
                Name = settings.Name,
                Phone = settings.Phone,
                VehicleTypeId = settings.VehicleTypeId,
                ChargeTypeId = settings.ChargeTypeId,
                ProviderId = settings.ProviderId
            };

            QueueCommand<IAccountServiceClient> (service =>
            {                     
                service.UpdateBookingSettings (bsr);
                
            });
            var account = CurrentAccount;
            account.Settings = settings;
            //Set to update the cache
            CurrentAccount = account;

        }

        public string UpdatePassword (Guid accountId, string currentPassword, string newPassword)
        {
            string response = null;
            response = UseServiceClient<IAccountServiceClient> (service => {                     
                service.UpdatePassword (new UpdatePassword () { AccountId = accountId, CurrentPassword = currentPassword, NewPassword = newPassword });
            }, ex => { throw ex; });

            return response;
        }

        public Account GetAccount (string email, string password)
        {
            try {
                var auth = TinyIoCContainer.Current.Resolve<IAuthServiceClient> ();
                var authResponse = auth.Authenticate (email, password);
                SaveCredentials (authResponse);                
                return GetAccount (true);
            } catch (WebException ex) {
                TinyIoC.TinyIoCContainer.Current.Resolve<IErrorHandler> ().HandleError (ex);
                
                return null;
            }
        }

        private static void SaveCredentials (AuthenticationData authResponse)
        {
            var cache = TinyIoC.TinyIoCContainer.Current.Resolve<ICacheService> ();
            cache.Set ("AuthenticationData", authResponse);
        }

        public Account GetFacebookAccount (string facebookId)
        {
            try {
                var auth = TinyIoCContainer.Current.Resolve<IAuthServiceClient> ();
                var authResponse = auth.AuthenticateFacebook (facebookId);
                SaveCredentials (authResponse);
                return GetAccount (false);
            } catch {
                return null;
            }
        }

        public Account GetTwitterAccount (string twitterId)
        {
            try {
                var parameters = new NamedParameterOverloads ();
                var auth = TinyIoCContainer.Current.Resolve<IAuthServiceClient> ();
                var authResponse = auth.AuthenticateTwitter (twitterId);
                SaveCredentials (authResponse);

                parameters.Add ("credential", authResponse);
                return GetAccount (false);
            } catch {
                return null;
            }
        }

        private Account GetAccount (bool showInvalidMessage)
        {
            Account data = null;

            try {
                TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_historyAddressesCacheKey);
                TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_favoriteAddressesCacheKey);

                var service = TinyIoCContainer.Current.Resolve<IAccountServiceClient> ("Authenticate");
                var account = service.GetMyAccount ();
                if (account != null) {
                    CurrentAccount = account;
                    data = account;
                }
                EnsureListLoaded ();
            } catch (WebException ex) {
                TinyIoC.TinyIoCContainer.Current.Resolve<IErrorHandler> ().HandleError (ex);
                return null;
            } catch (Exception e) {
                if (showInvalidMessage) {
                    var title = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("InvalidLoginMessageTitle");
                    var message = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("InvalidLoginMessage");
                    TinyIoCContainer.Current.Resolve<IMessageService> ().ShowMessage (title, message);
                }

                return null;
            }

            return data;
        }

        public void ResetPassword (string email)
        {
            UseServiceClient<IAccountServiceClient> ("NotAuthenticated", service => {               
                service.ResetPassword (email);               
            }, ex => { throw ex; });  
        }

        public bool Register (RegisterAccount data, out string error)
        {

            bool isSuccess = false;

            string lError = "";

            data.AccountId = Guid.NewGuid ();
            data.Language = TinyIoCContainer.Current.Resolve<IAppResource> ().CurrentLanguageCode;

            try {
                lError = UseServiceClient<IAccountServiceClient> (service =>
                {
                    service.RegisterAccount (data);
                    isSuccess = true;
                }
                );                
            } catch (Exception ex) {
                lError = ex.Message;
                isSuccess = false;
            }

            error = lError;
            return isSuccess;
        }

        public void DeleteFavoriteAddress (Guid addressId)
        {
            if (addressId.HasValue ()) {
                var toDelete = addressId;
                
                RemoveFromCacheArray<Address> (_favoriteAddressesCacheKey, toDelete, (id, a) => a.Id == id);                

                QueueCommand<IAccountServiceClient> (service => service.RemoveFavoriteAddress (toDelete));
            }
        }

        public void DeleteHistoryAddress (Guid addressId)
        {
            if (addressId.HasValue ()) {
                var toDelete = addressId;

                RemoveFromCacheArray<Address> (_historyAddressesCacheKey, toDelete, (id, a) => a.Id == id);

                QueueCommand<IAccountServiceClient> (service => service.RemoveAddress (toDelete));
            }
        }

        public void UpdateAddress (Address address)
        {
            bool isNew = address.Id.IsNullOrEmpty ();
            if (isNew) {
                
                address.Id = Guid.NewGuid ();
            }

            if (address.IsHistoric)
            {
                address.IsHistoric = false;
                RemoveFromCacheArray<Address>(_historyAddressesCacheKey, address.Id, (id, a) => a.Id == id);
            }
            UpdateCacheArray (_favoriteAddressesCacheKey, address, (a1, a2) => a1.Id.Equals (a2.Id));


            QueueCommand<IAccountServiceClient> (service =>
            {
                var toSave = new SaveAddress
                    {
                        Id = address.Id,
                        Address = address
                    };

                var toMove = toSave;

                if (isNew) {                        
                    service.AddFavoriteAddress (toSave);
                } else if (address.IsHistoric) {
                    service.UpdateFavoriteAddress (toMove);
                } else {
                    service.UpdateFavoriteAddress (toSave);
                }

            }
            );
        }

        public IEnumerable<ListItem> GetCompaniesList ()
        {
            EnsureListLoaded ();
            return _refData.CompaniesList;
        }

        public IEnumerable<ListItem> GetVehiclesList ()
        {
            EnsureListLoaded ();
            return _refData.VehiclesList;
        }

        public IEnumerable<ListItem> GetPaymentsList ()
        {
            EnsureListLoaded ();
            return _refData.PaymentsList;
        }

        public void AddCreditCard (CreditCardInfos creditCard)
        {
            var creditAuthorizationService = TinyIoCContainer.Current.Resolve<ICreditCardAuthorizationService>();

            creditCard.Token = creditAuthorizationService.Authorize(creditCard);

            var request = new CreditCardRequest
            {
                CreditCardCompany = creditCard.CreditCardCompany,
                CreditCardId = creditCard.CreditCardId,
                FriendlyName = creditCard.FriendlyName,
                Last4Digits = creditCard.Last4Digits,
                Token = creditCard.Token
            };

            UseServiceClient<IAccountServiceClient> (client => {               
                client.AddCreditCard(request);               
            }, ex => { throw ex; });  

        }
    }
}


