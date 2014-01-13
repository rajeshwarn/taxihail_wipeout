using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Data;
using ServiceStack.Common;
using apcurium.MK.Common.Configuration;
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
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Net;
using Position = apcurium.MK.Booking.Maps.Geo.Position;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class AccountService : BaseService, IAccountService
    {
        private const string FavoriteAddressesCacheKey = "Account.FavoriteAddresses";
        private const string HistoryAddressesCacheKey = "Account.HistoryAddresses";
        private const string RefDataCacheKey = "Account.ReferenceData";
        protected IConfigurationManager Config
        {
            get { return TinyIoCContainer.Current.Resolve < IConfigurationManager>(); }
        }
	
        [Obsolete("User async method instead")]
        public ReferenceData GetReferenceData()
        {
            var cached = Cache.Get<ReferenceData>(RefDataCacheKey);

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
            var cached = Cache.Get<ReferenceData>(RefDataCacheKey);

            if (cached == null)
            {
                var refData = UseServiceClient<ReferenceDataServiceClient, ReferenceData>(service => service.GetReferenceData());
                Cache.Set(RefDataCacheKey, await refData, DateTime.Now.AddHours(1));
                return await refData;

            }
            return cached;
        }

        public void ClearReferenceData()
        {
            Cache.Clear(RefDataCacheKey);
        }

        public void ResendConfirmationEmail (string email)
        {

        }

        public void ClearCache ()
        {
            var serverUrl = TinyIoCContainer.Current.Resolve<IAppSettings> ().ServiceUrl;
            Cache.Clear (HistoryAddressesCacheKey);
            Cache.Clear (FavoriteAddressesCacheKey);
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
            Cache.Clear (HistoryAddressesCacheKey);
            Cache.Clear (FavoriteAddressesCacheKey);

            if (reload) {
                GetFavoriteAddresses();
                GetHistoryAddresses();
            }
        }

        public IEnumerable<Address> GetHistoryAddresses()
        {
            var cached = Cache.Get<Address[]> (HistoryAddressesCacheKey);
            if (cached != null) {
                return cached;
            }

            var result = UseServiceClientAsync<IAccountServiceClient, IList<Address>>(service => service.GetHistoryAddresses (CurrentAccount.Id));

            Cache.Set(HistoryAddressesCacheKey, result.ToArray());
            return result;
        }

        public Task<IList<Order>> GetHistoryOrders ()
        {
            var client = TinyIoCContainer.Current.Resolve<OrderServiceClient>();
            return client.GetOrders();
        }

        public Order GetHistoryOrder (Guid id)
        {
            var result = 
            UseServiceClientAsync<OrderServiceClient, Order> (service => service.GetOrder (id));
            return result;
        }

        public OrderStatusDetail[] GetActiveOrdersStatus()
        {
            var result = UseServiceClientAsync<OrderServiceClient, OrderStatusDetail[]>(service => service.GetActiveOrdersStatus());
            return result;
        }

        public IEnumerable<Address> GetFavoriteAddresses ()
        {
            var cached = Cache.Get<Address[]> (FavoriteAddressesCacheKey);

            if (cached != null) {
                return cached;
            }
            var result = UseServiceClientAsync<IAccountServiceClient, IEnumerable<Address>>(service => service.GetFavoriteAddresses());
            var favoriteAddresses = result as Address[] ?? result.ToArray();
            Cache.Set (FavoriteAddressesCacheKey, favoriteAddresses.ToArray ());
            return favoriteAddresses;
        }

        private void UpdateCacheArray<T> (string key, T updated, Func<T, T, bool> compare) where T : class
        {
            var cached = Cache.Get<T[]> (key);

            if (cached != null) {

                var found = cached.SingleOrDefault (c => compare (updated, c));
                if (found == null) {
                    var newList = new T[cached.Length + 1];
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

        private void RemoveFromCacheArray<T> (string key, Guid toDeleteId, Func<Guid, T, bool> compare)
        {
            var cached = Cache.Get<T[]> (key);

            if ((cached != null) && (cached.Length > 0)) {
                var list = new List<T> (cached);
                var toDelete = list.SingleOrDefault (item => compare (toDeleteId, item));
                list.Remove (toDelete);
                Cache.Set (key, list.ToArray ());
            }
        }

        public Address FindInAccountAddresses (double latitude, double longitude)
        {
            var found = GetAddresseInRange(GetFavoriteAddresses(), new Position(latitude, longitude), 100) ??
                            GetAddresseInRange(GetHistoryAddresses(), new Position(latitude, longitude), 75);
            return found;

        }

        private Address GetAddresseInRange (IEnumerable<Address> addresses, Position position, float range)
        {
            var addressesInRange = from a in addresses
                let distance = position.DistanceTo (new Position (a.Latitude, a.Longitude))
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
            string response = UseServiceClient<IAccountServiceClient> (service => service.UpdatePassword (new UpdatePassword{ AccountId = accountId, CurrentPassword = currentPassword, NewPassword = newPassword }), ex => { throw ex; });

            return response;
        }

        public Account GetAccount (string email, string password)
        {
            try {
                var authResponse = UseServiceClientAsync<IAuthServiceClient, AuthenticationData>(service => service.Authenticate (email, password));
                SaveCredentials (authResponse);                
                return GetAccount (true);
            } catch (Exception ex) {
				if (ex is WebException || (ex is WebServiceException && ((WebServiceException)ex).StatusCode == (int)HttpStatusCode.NotFound)) {
                    var title = TinyIoCContainer.Current.Resolve<ILocalization>()["NoConnectionTitle"];
                    var msg = TinyIoCContainer.Current.Resolve<ILocalization>()["NoConnectionMessage"];
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
                var authResponse = UseServiceClientAsync<IAuthServiceClient, AuthenticationData>(service => service.AuthenticateTwitter(twitterId));
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
                Cache.Clear (HistoryAddressesCacheKey);
                Cache.Clear (FavoriteAddressesCacheKey);

                var account = UseServiceClientAsync<IAccountServiceClient, Account>(service => service.GetMyAccount ());
                if (account != null) {
                    CurrentAccount = account;
                    data = account;
                }
                
            } catch (WebException ex) {
                TinyIoCContainer.Current.Resolve<IErrorHandler> ().HandleError (ex);
                return null;
			} catch{
                if (showInvalidMessage) {
                    var title = TinyIoCContainer.Current.Resolve<ILocalization>()["InvalidLoginMessageTitle"];
                    var message = TinyIoCContainer.Current.Resolve<ILocalization>()["InvalidLoginMessage"];
                    TinyIoCContainer.Current.Resolve<IMessageService> ().ShowMessage (title, message);
                }

                return null;
            }

            return data;
        }

        public Account RefreshAccount ()
        {
            try {
                var account = UseServiceClientAsync<IAccountServiceClient, Account>(service => service.GetMyAccount());
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
            string lError;

            data.AccountId = Guid.NewGuid();
            data.Language = TinyIoCContainer.Current.Resolve<ILocalization>()["LanguageCode"];

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
            if (addressId.HasValue()) {
                var toDelete = addressId;
                
                RemoveFromCacheArray<Address> (FavoriteAddressesCacheKey, toDelete, (id, a) => a.Id == id);                

                UseServiceClient<IAccountServiceClient> (service => service.RemoveFavoriteAddress (toDelete));
            }
        }

        public void DeleteHistoryAddress (Guid addressId)
        {
            if (addressId.HasValue ()) {
                var toDelete = addressId;

                RemoveFromCacheArray<Address> (HistoryAddressesCacheKey, toDelete, (id, a) => a.Id == id);

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
                RemoveFromCacheArray<Address> (HistoryAddressesCacheKey, address.Id, (id, a) => a.Id == id);
            }
            UpdateCacheArray (FavoriteAddressesCacheKey, address, (a1, a2) => a1.Id.Equals (a2.Id));


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
            if (!Config.GetSetting("Client.HideNoPreference", false)
                && refData.CompaniesList != null)
            {
                refData.CompaniesList.Insert(0, new ListItem
                                            {
                    Id = null,
                    Display = TinyIoCContainer.Current.Resolve<ILocalization>()["NoPreference"]
                });
            }
            
            return refData.CompaniesList;         
        }

        public IEnumerable<ListItem> GetVehiclesList ()
        {
            var refData = GetReferenceData();

            if (!Config.GetSetting("Client.HideNoPreference", false)
                && refData.VehiclesList != null)
            {
                refData.VehiclesList.Insert(0, new ListItem
                                         {
                                            Id = null,
                                            Display = TinyIoCContainer.Current.Resolve<ILocalization>()["NoPreference"]
                                         });
            }

            return refData.VehiclesList;
        }

        public IEnumerable<ListItem> GetPaymentsList ()
        {
            var refData = GetReferenceData();
		
		    if (!Config.GetSetting("Client.HideNoPreference", false)
                && refData.PaymentsList != null)
            {
                refData.PaymentsList.Insert(0, new ListItem
                {
                    Id = null,
                    Display = TinyIoCContainer.Current.Resolve<ILocalization>()["NoPreference"]
                });
            }

            return refData.PaymentsList;
        }

        public IEnumerable<CreditCardDetails> GetCreditCards ()
        {
            var result = UseServiceClientAsync<IAccountServiceClient, IEnumerable<CreditCardDetails>>(service => service.GetCreditCards());
            return result;
        }

        public void RemoveCreditCard (Guid creditCardId)
        {
            UseServiceClient<IAccountServiceClient>(client => client.RemoveCreditCard(creditCardId,""), ex => { throw ex; });
            
        }

        public bool AddCreditCard (CreditCardInfos creditCard)
        {
			try
			{
                var response = UseServiceClientAsync<IPaymentService, TokenizedCreditCardResponse>(service => service.Tokenize(
                    creditCard.CardNumber,
                    new DateTime(creditCard.ExpirationYear.ToInt(), creditCard.ExpirationMonth.ToInt(), 1),
                    creditCard.CCV));	
			    creditCard.Token = response.CardOnFileToken;       
			
			}
			catch
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
            
            UseServiceClient<IAccountServiceClient> (client => client.AddCreditCard (request), ex => {
                throw ex; });  

			return true;

        }



    }
}


