using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.Data;
using MK.Booking.Api.Client;
using ServiceStack.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Mobile.AppServices.Social;

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
        private const string _refDataCacheKey = "Account.ReferenceData";


        protected IConfigurationManager Config
        {
            get { return TinyIoCContainer.Current.Resolve < IConfigurationManager>(); }
        }

        protected ICacheService Cache
        {
            get { return TinyIoCContainer.Current.Resolve < ICacheService>(); }
        }


        [Obsolete("User async method instead")]
        public ReferenceData GetReferenceData()
        {
            var cached = Cache.Get<ReferenceData>(_refDataCacheKey);

            if (cached == null)
            {
                var referenceData = GetReferenceDataAsync();
                referenceData.Start();
//                if (!referenceData.IsCompleted)
//                {
//                    Task.WaitAll(referenceData);
//                }
                return referenceData.Result;
            }
            return cached;

        }

        public async Task<ReferenceData> GetReferenceDataAsync()
        {
            var cached = Cache.Get<ReferenceData>(_refDataCacheKey);

            if (cached == null)
            {
                var refData = UseServiceClient<ReferenceDataServiceClient, ReferenceData>(service => service.GetReferenceData());
                Cache.Set(_refDataCacheKey, await refData, DateTime.Now.AddHours(1));
                return await refData;

            }
            return cached;
        }

        public void ClearReferenceData()
        {
            Cache.Clear(_refDataCacheKey);
        }

        public void ResendConfirmationEmail (string email)
        {

        }

        public void ClearCache ()
        {
            var serverUrl = TinyIoCContainer.Current.Resolve<IAppSettings> ().ServiceUrl;


            
            Cache.Clear (_historyAddressesCacheKey);
            Cache.Clear (_favoriteAddressesCacheKey);
            Cache.Clear ("AuthenticationData");
            Cache.ClearAll ();
            TinyIoCContainer.Current.Resolve<IAppSettings> ().ServiceUrl = serverUrl; 
        }

        public void SignOut ()
        {
            try
			{
                var facebook = TinyIoCContainer.Current.Resolve<IFacebookService> ();
                facebook.Disconnect ();
            } 
			catch( Exception ex )
            {
                Console.WriteLine(ex.Message);
            }

            try
			{
                var twitterService = TinyIoCContainer.Current.Resolve<ITwitterService> ();
                if (twitterService.IsConnected)
				{
                    twitterService.Disconnect ();
                }
            } 
            catch( Exception ex )
            {
                Console.WriteLine(ex.Message);
            }


            ClearCache ();
          
        }

        public void RefreshCache (bool reload)
        {
            Cache.Clear (_historyAddressesCacheKey);
            Cache.Clear (_favoriteAddressesCacheKey);

            if (reload) {
                GetFavoriteAddresses ();
                GetHistoryAddresses ();
            }
        }

        public IEnumerable<Address> GetHistoryAddresses ()
        {
            var cached = Cache.Get<Address[]> (_historyAddressesCacheKey);
            if (cached != null) {
                return cached;
            } else {

                IEnumerable<Address> result = new Address[0];
                UseServiceClient<IAccountServiceClient> (service =>
                {
                    result = service.GetHistoryAddresses (CurrentAccount.Id);
                }
                );

                Cache.Set (_historyAddressesCacheKey, result.ToArray ());
                return result;
            }
        }

        public Task<Order[]> GetHistoryOrders ()
        {
            return Task.Factory.StartNew(() =>
                {

                    var result = new Order[0];
                    UseServiceClient<OrderServiceClient>(service =>
                        {
                            result = service.GetOrders().ToArray();
                        }
                        );
                    return result;
                });
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
            var cached = Cache.Get<Address[]> (_favoriteAddressesCacheKey);

            if (cached != null) {
                return cached;
            } else {

                IEnumerable<Address> result = new Address[0];
                UseServiceClient<IAccountServiceClient> (service =>
                {
                    result = service.GetFavoriteAddresses ();
                }
                );
                Cache.Set (_favoriteAddressesCacheKey, result.ToArray ());
                return result;
            }
        }

        private void UpdateCacheArray<T> (string key, T updated, Func<T, T, bool> compare)
        {
            var cached = Cache.Get<T[]> (key);

            if (cached != null) {

                var found = cached.SingleOrDefault (c => compare (updated, c));
                if (found == null) {
                    T[] newList = new T[cached.Length + 1];
                    Array.Copy (cached, newList, cached.Length);
                    newList [cached.Length] = updated;

                    Cache.Set (key, newList);
                } else {
                    var foundIndex = cached.IndexOf (updated, compare);
                    cached [foundIndex] = updated;
                    Cache.Set (key, cached);
                }
            }


        }

        private bool RemoveFromCacheArray<T> (string key, Guid toDeleteId, Func<Guid, T, bool> compare)
        {
            var cached = Cache.Get<T[]> (key);

            if ((cached != null) && (cached.Length > 0)) {
                var list = new List<T> (cached);
                var toDelete = list.SingleOrDefault (item => compare (toDeleteId, item));
                var removed = list.Remove (toDelete);
                Cache.Set (key, list.ToArray ());
                return removed;
            }
            return false;
        }

        public Address FindInAccountAddresses (double latitude, double longitude)
        {
            Address found = GetAddresseInRange(GetFavoriteAddresses(), new apcurium.MK.Booking.Maps.Geo.Position(latitude, longitude), 100);                   
            if (found == null)
            {
                found = GetAddresseInRange(GetHistoryAddresses(), new apcurium.MK.Booking.Maps.Geo.Position(latitude, longitude), 75);
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

                var account = Cache.Get<Account> ("LoggedUser");
                return account;
            }
            private set {
                if (value != null) {
                    Cache.Set ("LoggedUser", value);    
                } else {
                    Cache.Clear ("LoggedUser");
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
            
            UseServiceClient<IAccountServiceClient> (service =>
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
            } catch (Exception ex) {
				if (ex is WebException || (ex is WebServiceException && ((WebServiceException)ex).StatusCode == (int)HttpStatusCode.NotFound)) {
                    var title = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoConnectionTitle");
                    var msg = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoConnectionMessage");
                    var mService = TinyIoCContainer.Current.Resolve<IMessageService> ();
                    mService.ShowMessage (title, msg);
					return null;
				}
				throw;
            }
        }

        private static void SaveCredentials (AuthenticationData authResponse)
        {         
            TinyIoCContainer.Current.Resolve < ICacheService>().Set ("AuthenticationData", authResponse);
        }

		public async Task<Account> GetFacebookAccount (string facebookId)
        {
            try {
                var auth = TinyIoCContainer.Current.Resolve<IAuthServiceClient> ();
				var authResponse = auth
					.AuthenticateFacebook(facebookId)
					.ConfigureAwait(false);

				SaveCredentials (await authResponse);

                return GetAccount (false);

			} catch(Exception e) {
				Logger.LogError(e);
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
                Cache.Clear (_historyAddressesCacheKey);
                Cache.Clear (_favoriteAddressesCacheKey);

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

                UseServiceClient<IAccountServiceClient> (service => service.RemoveFavoriteAddress (toDelete));
            }
        }

        public void DeleteHistoryAddress (Guid addressId)
        {
            if (addressId.HasValue ()) {
                var toDelete = addressId;

                RemoveFromCacheArray<Address> (_historyAddressesCacheKey, toDelete, (id, a) => a.Id == id);

                UseServiceClient<IAccountServiceClient> (service => service.RemoveAddress (toDelete));
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


            UseServiceClient<IAccountServiceClient> (service =>
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



            if (!Config.GetSetting<bool>("Client.HideNoPreference", false)
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

            if (!Config.GetSetting<bool>("Client.HideNoPreference", false)
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

            var settings = TinyIoCContainer.Current.Resolve<IConfigurationManager> ().GetPaymentSettings ();
			var paymentsEnabled = settings.IsPayInTaxiEnabled || settings.PayPalClientSettings.IsEnabled;
			           
            if (!Config.GetSetting<bool>("Client.HideNoPreference", false)
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

            IEnumerable<CreditCardDetails> result = new CreditCardDetails[0];
            UseServiceClient<IAccountServiceClient> (service =>
            {
                result = service.GetCreditCards ();
            });
        
            return result;
            
        }

        public void RemoveCreditCard (Guid creditCardId)
        {
            UseServiceClient<IAccountServiceClient>(client => client.RemoveCreditCard(creditCardId,""), ex => { throw ex; });
            
        }

        public bool AddCreditCard (CreditCardInfos creditCard)
        {
            var creditAuthorizationService = TinyIoCContainer.Current.Resolve<IPaymentService> ();
            
			try
			{

                var response = creditAuthorizationService.Tokenize(
                    creditCard.CardNumber,
                    new DateTime(creditCard.ExpirationYear.ToInt(), creditCard.ExpirationMonth.ToInt(), 1),
                    creditCard.CCV); 				
			    creditCard.Token = response.CardOnFileToken;       
			
			}
			catch(Exception e)
			{
                return false;
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
            }, ex => {
                throw ex; });  

			return true;

        }



    }
}


