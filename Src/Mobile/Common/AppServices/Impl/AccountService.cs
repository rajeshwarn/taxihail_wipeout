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
using Cirrious.CrossCore;
using TinyIoC;
using apcurium.MK.Common.Enumeration;


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

		readonly IAppSettings _appSettings;
		readonly IFacebookService _facebookService;
		readonly ITwitterService _twitterService;
		readonly ILocalization _localize;

		public AccountService(IAppSettings appSettings,
			IFacebookService facebookService,
			ITwitterService twitterService,
			ILocalization localize)
		{
			_localize = localize;
			_twitterService = twitterService;
			_facebookService = facebookService;
			_appSettings = appSettings;

		}
	
        public async Task<ReferenceData> GetReferenceData()
        {
            var cached = Cache.Get<ReferenceData>(RefDataCacheKey);

            if (cached == null)
            {
                var refData = UseServiceClientAsync<ReferenceDataServiceClient, ReferenceData>(service => service.GetReferenceData());
                Cache.Set(RefDataCacheKey, await refData, DateTime.Now.AddHours(1));
                return await refData;

            }
            return cached;
        }

        public void ClearReferenceData()
        {
            Cache.Clear(RefDataCacheKey);
        }

        public void ClearCache ()
        {
			var serverUrl = _appSettings.Data.ServiceUrl;

            Cache.Clear (HistoryAddressesCacheKey);
            Cache.Clear (FavoriteAddressesCacheKey);
            Cache.Clear ("AuthenticationData");
            Cache.ClearAll ();
			// TODO: Clearing the cache should not clear ServiceUrl
			_appSettings.Data.ServiceUrl = serverUrl; 
        }

        public void SignOut ()
        {
            try
			{
				if(_appSettings.Data.FacebookEnabled)
				{
					_facebookService.Disconnect ();
				}
            } 
			catch( Exception ex )
            {
				Logger.LogError(ex);
            }

            try
			{
				if (_appSettings.Data.TwitterEnabled
					&& _twitterService.IsConnected)
				{
					_twitterService.Disconnect ();
                }
            } 
            catch( Exception ex )
            {
				Logger.LogError(ex);
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

            var result = UseServiceClientTask<IAccountServiceClient, IList<Address>>(service => service.GetHistoryAddresses (CurrentAccount.Id));

            Cache.Set(HistoryAddressesCacheKey, result.ToArray());
            return result;
        }

        public Task<IList<Order>> GetHistoryOrders ()
        {
			var client = Mvx.Resolve<OrderServiceClient>();
            return client.GetOrders();
        }

		public Order GetHistoryOrder (Guid id)
        {
			// TODO: remove ussage of this method in favor of async version
			var task = GetHistoryOrderAsync(id);
			task.Wait();
			return task.Result;
        }

		public Task<Order> GetHistoryOrderAsync (Guid id)
		{
			return UseServiceClientAsync<OrderServiceClient, Order> (service => service.GetOrder (id));
		}

        public OrderStatusDetail[] GetActiveOrdersStatus()
        {
            var result = UseServiceClientTask<OrderServiceClient, OrderStatusDetail[]>(service => service.GetActiveOrdersStatus());
            return result;
        }

        public IEnumerable<Address> GetFavoriteAddresses ()
        {
            var cached = Cache.Get<Address[]> (FavoriteAddressesCacheKey);

            if (cached != null) {
                return cached;
            }
            var result = UseServiceClientTask<IAccountServiceClient, IEnumerable<Address>>(service => service.GetFavoriteAddresses());
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
            
			UseServiceClientTask<IAccountServiceClient> (service =>                                                 {                     
				return service.UpdateBookingSettings (bsr);                
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
			var response = UseServiceClientTask<IAccountServiceClient, string> (service => service.UpdatePassword (new UpdatePassword{ AccountId = accountId, CurrentPassword = currentPassword, NewPassword = newPassword }));

            return response;
        }

		public async Task<Account> SignIn (string email, string password)
        {
			Logger.LogMessage("SignIn with server {0}", _appSettings.Data.ServiceUrl);
            try 
			{
				var authResponse = await UseServiceClientAsync<IAuthServiceClient, AuthenticationData>(service => service
					.Authenticate (email, password),
					error => { /* Avoid trigerring global error handler */ });
                SaveCredentials (authResponse);                
                return await GetAccount ();
            }
            catch(WebException e)
            {
                // Happen when device is not connected
                throw new AuthException("Network error", AuthFailure.NetworkError, e);
            }
            catch(WebServiceException e)
            {
                if (e.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    throw new AuthException("Invalid service url", AuthFailure.InvalidServiceUrl, e);
                }
                else if (e.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    throw new AuthException("Invalid username or password", AuthFailure.InvalidUsernameOrPassword, e);
                }
                throw;
            }
            catch (Exception e)
            {
                if(e.Message == AuthenticationErrorCode.AccountDisabled)
                {
                    throw new AuthException("Account disabled", AuthFailure.AccountDisabled, e);
                }
                throw;
            }
        }

        private static void SaveCredentials (AuthenticationData authResponse)
        {         
			Mvx.Resolve<ICacheService>().Set ("AuthenticationData", authResponse);
        }

		public async Task<Account> GetFacebookAccount (string facebookId)
        {
            try
			{
				var auth = Mvx.Resolve<IAuthServiceClient> ();
				var authResponse = auth
					.AuthenticateFacebook(facebookId)
					.ConfigureAwait(false);

				SaveCredentials (await authResponse);

				return await GetAccount ();

			}
			catch(Exception e)
			{
				Logger.LogError(e);
				throw;
            }
        }

		public async Task<Account> GetTwitterAccount (string twitterId)
        {
            try
			{
				var authResponse = await UseServiceClientAsync<IAuthServiceClient, AuthenticationData>(service => service.AuthenticateTwitter(twitterId), e => {});
                SaveCredentials (authResponse);

				return await GetAccount ();
            }
			catch(Exception e)
			{
				Logger.LogError(e);
				throw;
            }
        }

		private async Task<Account> GetAccount ()
        {
            Account data = null;

            try
			{
                //todo avoir une cache propre au login du user
                Cache.Clear (HistoryAddressesCacheKey);
                Cache.Clear (FavoriteAddressesCacheKey);

				var account = await UseServiceClientAsync<IAccountServiceClient, Account>(service => service.GetMyAccount ());
                if (account != null)
				{
                    CurrentAccount = account;
                    data = account;
                }
                
            }
			catch (WebException ex)
			{
				Mvx.Resolve<IErrorHandler>().HandleError (ex);
                return null;
			}
			catch
			{
                return null;
            }

            return data;
        }

        public Account RefreshAccount ()
        {
            try {
                var account = UseServiceClientTask<IAccountServiceClient, Account>(service => service.GetMyAccount());
                CurrentAccount = account;
                return account;
            } catch {
                return null;
            }
        }

        public void ResetPassword (string email)
        {
			UseServiceClientTask<IAccountServiceClient> (service => service.ResetPassword (email));  
        }

		public async Task Register (RegisterAccount data)
        {
            data.AccountId = Guid.NewGuid();
			data.Language = _localize["LanguageCode"];
			await UseServiceClientAsync<IAccountServiceClient> (service =>  service.RegisterAccount (data)); 
        }

        public void DeleteFavoriteAddress (Guid addressId)
        {
            if (addressId.HasValue()) {
                var toDelete = addressId;
                
                RemoveFromCacheArray<Address> (FavoriteAddressesCacheKey, toDelete, (id, a) => a.Id == id);                

				UseServiceClientTask<IAccountServiceClient> (service => service.RemoveFavoriteAddress (toDelete));
            }
        }

        public void DeleteHistoryAddress (Guid addressId)
        {
            if (addressId.HasValue ()) {
                var toDelete = addressId;

                RemoveFromCacheArray<Address> (HistoryAddressesCacheKey, toDelete, (id, a) => a.Id == id);

				UseServiceClientTask<IAccountServiceClient> (service => service.RemoveAddress (toDelete));
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


			UseServiceClientTask<IAccountServiceClient> (service =>
            {
                var toSave = new SaveAddress
                    {
                        Id = address.Id,
                        Address = address
                    };

                var toMove = toSave;

                if (isNew) {                        
				  return service.AddFavoriteAddress (toSave);
                } else if (address.IsHistoric) {
				  return service.UpdateFavoriteAddress (toMove);
                } else {
			      return service.UpdateFavoriteAddress (toSave);
                }

            }
            );
        }

		public async Task<IList<ListItem>> GetCompaniesList ()
        {
			var refData = await GetReferenceData();
			if (!_appSettings.Data.HideNoPreference
                && refData.CompaniesList != null)
            {
				refData.CompaniesList.Insert(0,
					new ListItem
					{
						Id = null,
						Display = _localize["NoPreference"]
					});
            }
            
            return refData.CompaniesList;         
        }

		public async Task<IList<ListItem>> GetVehiclesList ()
        {
			var refData = await GetReferenceData();

			if (!_appSettings.Data.HideNoPreference
                && refData.VehiclesList != null)
            {
                refData.VehiclesList.Insert(0,
					new ListItem
                    {
                        Id = null,
						Display = _localize["NoPreference"]
					});
            }

            return refData.VehiclesList;
        }

		public async Task<IList<ListItem>> GetPaymentsList ()
        {
			var refData = await GetReferenceData();
		
			if (!_appSettings.Data.HideNoPreference
                && refData.PaymentsList != null)
            {
                refData.PaymentsList.Insert(0,
					new ListItem
                	{
                        Id = null,
					    Display = _localize["NoPreference"]
                	});
            }

            return refData.PaymentsList;
        }

        public IEnumerable<CreditCardDetails> GetCreditCards ()
        {
            var result = UseServiceClientTask<IAccountServiceClient, IEnumerable<CreditCardDetails>>(service => service.GetCreditCards());
            return result;
        }

        public void RemoveCreditCard (Guid creditCardId)
        {
			UseServiceClientTask<IAccountServiceClient>(client => client.RemoveCreditCard(creditCardId,""));
        }

		public async Task<bool> AddCreditCard (CreditCardInfos creditCard)
        {
			try
			{
				var response = await UseServiceClientAsync<IPaymentService, TokenizedCreditCardResponse>(service => service.Tokenize(
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
            
			await UseServiceClientAsync<IAccountServiceClient> (client => client.AddCreditCard (request));  

			return true;

        }
    }
}


