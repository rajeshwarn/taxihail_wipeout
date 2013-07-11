using System;
using System.Linq;
using System.Collections.Generic;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using SocialNetworks.Services;
using apcurium.MK.Booking.Mobile.Data;
using MK.Booking.Api.Client;
using ServiceStack.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

#if IOS
using ServiceStack.ServiceClient.Web;
using ServiceStack.Common.ServiceClient.Web;
#endif
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
        private const string _creditCardsCacheKey = "Account.CreditCards";
        private const string _refDataCacheKey = "Account.ReferenceData";

        public ReferenceData GetReferenceData()
        {

            var refData = TinyIoCContainer.Current.Resolve<IAppCacheService>().Get<ReferenceData>(_refDataCacheKey);
            
            if (refData == null)
            {
                UseServiceClient<ReferenceDataServiceClient>(service =>
                {
                    refData = service.GetReferenceData();
                    TinyIoCContainer.Current.Resolve<IAppCacheService>().Set(_refDataCacheKey, refData, DateTime.Now.AddHours(1));
                });
            }
            return refData;
            

        }

        public void ClearReferenceData()
        {
            TinyIoCContainer.Current.Resolve<IAppCacheService>().Clear(_refDataCacheKey);
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


            
            TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_historyAddressesCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_favoriteAddressesCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_creditCardsCacheKey);
            TinyIoCContainer.Current.Resolve<ICacheService> ().Clear ("AuthenticationData");
            TinyIoCContainer.Current.Resolve<ICacheService> ().ClearAll ();
            TinyIoCContainer.Current.Resolve<IAppSettings> ().ServiceUrl = serverUrl; 
        }

        public void SignOut ()
        {
            try {
                var facebook = TinyIoCContainer.Current.Resolve<IFacebookService> ();
                if (facebook.IsConnected) {                    
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

        public OrderStatusDetail[] GetActiveOrdersStatus()
        {
            var result = default(OrderStatusDetail[]);
            UseServiceClient<OrderServiceClient>(service =>
            {
                result = service.GetActiveOrdersStatus();
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
            Address found = GetAddresseInRange (GetFavoriteAddresses (), new apcurium.MK.Booking.Maps.Geo.Position (latitude, longitude), 100);                   
            if (found == null) {
                found = GetAddresseInRange (GetHistoryAddresses (), new apcurium.MK.Booking.Maps.Geo.Position (latitude, longitude), 75);
            }
            return found;

        }

        private Address GetAddresseInRange (IEnumerable<Address> addresses, apcurium.MK.Booking.Maps.Geo.Position position, float range)
        {             
            const double R = 6378137;
            
            var addressesInRange = from a in addresses
                let distance = position.DistanceTo (new apcurium.MK.Booking.Maps.Geo.Position (a.Latitude, a.Longitude))
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
        public void UpdateSettings (BookingSettings settings, Guid? creditCardId, int? tipPercent)
        {
            var bsr = new BookingSettingsRequest
            {
                Name = settings.Name,
                Phone = settings.Phone,
                VehicleTypeId = settings.VehicleTypeId,
                ChargeTypeId = settings.ChargeTypeId,
                ProviderId = settings.ProviderId,
                DefaultCreditCard = creditCardId,
                DefaultTipPercent = tipPercent
            };
            
            QueueCommand<IAccountServiceClient> (service =>
                                                 {                     
                service.UpdateBookingSettings (bsr);
                
            });
            var account = CurrentAccount;
            account.Settings = settings;
            account.DefaultCreditCard = creditCardId;
            account.DefaultTipPercent = tipPercent;
            //Set to update the cache
            CurrentAccount = account;
            
        }
        public string UpdatePassword (Guid accountId, string currentPassword, string newPassword)
        {
            string response = null;
            response = UseServiceClient<IAccountServiceClient> (service => {                     
                service.UpdatePassword (new UpdatePassword () { AccountId = accountId, CurrentPassword = currentPassword, NewPassword = newPassword });
            }, ex => {
                throw ex; });

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

        public Account RefreshAccount ()
        {
            try {
                var service = TinyIoCContainer.Current.Resolve<IAccountServiceClient> ("Authenticate");
                var account = service.GetMyAccount ();
                CurrentAccount = account;

            
                return account;
            } catch {
                return null;
            }
        }

        public void ResetPassword (string email)
        {
            UseServiceClient<IAccountServiceClient> ("NotAuthenticated", service => {               
                service.ResetPassword (email);               
            }, ex => {
                throw ex; });  
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

            if (address.IsHistoric) {
                address.IsHistoric = false;
                RemoveFromCacheArray<Address> (_historyAddressesCacheKey, address.Id, (id, a) => a.Id == id);
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
            var refData = GetReferenceData();
            
            if (!TinyIoCContainer.Current.Resolve<IAppSettings> ().HideNoPreference
                && refData.CompaniesList != null)
            {
                refData.CompaniesList.Insert(0, new ListItem
                                            {
                    Id = null,
                    Display = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoPreference")
                });
            }
            
            return refData.CompaniesList;         
        }

        public IEnumerable<ListItem> GetVehiclesList ()
        {
            var refData = GetReferenceData();

            if (!TinyIoCContainer.Current.Resolve<IAppSettings> ().HideNoPreference
                && refData.VehiclesList != null)
            {
                refData.VehiclesList.Insert(0, new ListItem
                                         {
                                            Id = null,
                                            Display = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoPreference")
                                         });
            }

            return refData.VehiclesList;
        }

        public IEnumerable<ListItem> GetPaymentsList ()
        {
            var refData = GetReferenceData();
            var appSettings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
            //add credit card on file if not already included and feature enabled

			var settings = TinyIoCContainer.Current.Resolve<IConfigurationManager> ().GetPaymentSettings ();
			var paymentsEnabled = settings.PaymentMode != PaymentMethod.None || settings.PayPalClientSettings.IsEnabled;

            if (paymentsEnabled
                && refData.PaymentsList != null
                && refData.PaymentsList.None(x => x.Id == ReferenceData.CreditCardOnFileType))
            {

                refData.PaymentsList.Add(new ListItem
                          { 
                            Id = ReferenceData.CreditCardOnFileType, 
                            Display =  TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("ChargeTypeCreditCardFile")
                          });
            }

            
            if (!appSettings.HideNoPreference
                && refData.PaymentsList != null)
            {
                refData.PaymentsList.Insert(0, new ListItem
                {
                    Id = null,
                    Display = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoPreference")
                });
            }

            return refData.PaymentsList;
        }

        public IEnumerable<CreditCardDetails> GetCreditCards ()
        {
            var cache = TinyIoCContainer.Current.Resolve<ICacheService> ();
            var cached = cache.Get<CreditCardDetails[]> (_creditCardsCacheKey);
            
            if (cached != null) {
                return cached;
            } else {
                IEnumerable<CreditCardDetails> result = new CreditCardDetails[0];
                UseServiceClient<IAccountServiceClient> (service =>
                {
                    result = service.GetCreditCards ();
                });
                cache.Set (_creditCardsCacheKey, result.ToArray ());
                return result;
            }
        }

        public void RemoveCreditCard (Guid creditCardId)
        {
            UseServiceClient<IAccountServiceClient>(client => client.RemoveCreditCard(creditCardId,""), ex => { throw ex; });
            
            TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_creditCardsCacheKey);
        }

        public void AddCreditCard (CreditCardInfos creditCard)
        {
            var creditAuthorizationService = TinyIoCContainer.Current.Resolve<IPaymentServiceClient> ();
            
			try
			{

                var response = creditAuthorizationService.Tokenize(
                    creditCard.CardNumber,
                    new DateTime(creditCard.ExpirationYear.ToInt(), creditCard.ExpirationMonth.ToInt(), 1),
                    creditCard.CCV); 				
			    creditCard.Token = response.CardOnFileToken;       
			
			}
			catch
			{
                TinyIoCContainer.Current.Resolve<IMessageService> ().ShowMessage ( "Validation", "Cannot validate the credit card.");
                return;
			}
            
            
            var request = new CreditCardRequest
            {
                CreditCardCompany = creditCard.CreditCardCompany,
                CreditCardId = creditCard.CreditCardId,
                FriendlyName = creditCard.FriendlyName,
                Last4Digits = creditCard.Last4Digits,
                Token = creditCard.Token
            };
            
            UseServiceClient<IAccountServiceClient> (client => {               
                client.AddCreditCard (request); 
                TinyIoCContainer.Current.Resolve<ICacheService> ().Clear (_creditCardsCacheKey);
            }, ex => {
                throw ex; });  
        }



    }
}


