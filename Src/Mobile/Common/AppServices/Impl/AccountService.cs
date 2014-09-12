#if IOS
using ServiceStack.ServiceClient.Web;
using ServiceStack.Common.ServiceClient.Web;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Cirrious.CrossCore;
using MK.Common.Configuration;
using ServiceStack.Common;
using ServiceStack.ServiceClient.Web;
using Position = apcurium.MK.Booking.Maps.Geo.Position;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class AccountService : BaseService, IAccountService
    {
        private const string FavoriteAddressesCacheKey = "Account.FavoriteAddresses";
        private const string HistoryAddressesCacheKey = "Account.HistoryAddresses";
        private const string RefDataCacheKey = "Account.ReferenceData";
        private const string CompanyNotificationSettingsCacheKey = "Account.CompanyNotificationSettings";
        private const string UserNotificationSettingsCacheKey = "Account.UserNotificationSettings";
        private const string AuthenticationDataCacheKey = "AuthenticationData";

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
            var cached = UserCache.Get<ReferenceData>(RefDataCacheKey);

            if (cached == null)
            {
                var refData = UseServiceClientAsync<ReferenceDataServiceClient, ReferenceData>(service => service.GetReferenceData());
				UserCache.Set(RefDataCacheKey, await refData, DateTime.Now.AddHours(1));
				return await refData;
            }
            return cached;
        }

        public void ClearReferenceData()
        {
            UserCache.Clear(RefDataCacheKey);
        }

        public void ClearCache ()
        {
            UserCache.Clear (HistoryAddressesCacheKey);
            UserCache.Clear (FavoriteAddressesCacheKey);
            UserCache.Clear(CompanyNotificationSettingsCacheKey);
            UserCache.Clear(UserNotificationSettingsCacheKey);
            UserCache.Clear (AuthenticationDataCacheKey);
            UserCache.ClearAll ();
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

		public async void RefreshCache (bool reload)
        {
            UserCache.Clear (HistoryAddressesCacheKey);
            UserCache.Clear (FavoriteAddressesCacheKey);

            if (reload)
			{
				await Task.WhenAll(GetFavoriteAddresses(), GetHistoryAddresses());
            }
        }

		public async Task<Address[]> GetHistoryAddresses()
        {
            var cached = UserCache.Get<Address[]> (HistoryAddressesCacheKey);
            if (cached != null) 
			{
                return cached;
            }

			var result = await UseServiceClientAsync<IAccountServiceClient, IList<Address>>(s => s
				.GetHistoryAddresses (CurrentAccount.Id))
				.ConfigureAwait(false);

            UserCache.Set(HistoryAddressesCacheKey, result.ToArray());
			return result.ToArray();
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

		public async Task<Address[]> GetFavoriteAddresses ()
        {
            var cached = UserCache.Get<Address[]> (FavoriteAddressesCacheKey);

            if (cached != null)
			{
                return cached;
            }

			var result = await UseServiceClientAsync<IAccountServiceClient, IEnumerable<Address>>(s => s
				.GetFavoriteAddresses())
				.ConfigureAwait(false);

            var favoriteAddresses = result as Address[] ?? result.ToArray();
            UserCache.Set (FavoriteAddressesCacheKey, favoriteAddresses.ToArray ());

            return favoriteAddresses;
        }

        private void UpdateCacheArray<T> (string key, T updated, Func<T, T, bool> compare) where T : class
        {
            var cached = UserCache.Get<T[]> (key);

            if (cached != null) {

                var found = cached.SingleOrDefault (c => compare (updated, c));
                if (found == null) {
                    var newList = new T[cached.Length + 1];
                    Array.Copy (cached, newList, cached.Length);
                    newList [cached.Length] = updated;

                    UserCache.Set (key, newList);
                } else {
                    var foundIndex = cached.IndexOf (updated, compare);
                    cached [foundIndex] = updated;
                    UserCache.Set (key, cached);
                }
            }
        }

        private void RemoveFromCacheArray<T> (string key, Guid toDeleteId, Func<Guid, T, bool> compare)
        {
            var cached = UserCache.Get<T[]> (key);

            if ((cached != null) && (cached.Length > 0)) 
			{
                var list = new List<T> (cached);
                var toDelete = list.SingleOrDefault (item => compare (toDeleteId, item));
                list.Remove (toDelete);
                UserCache.Set (key, list.ToArray ());
            }
        }

		public async Task<Address> FindInAccountAddresses (double latitude, double longitude)
        {
			var found = GetAddressInRange(await GetFavoriteAddresses(), new Position(latitude, longitude), 100) 
				?? GetAddressInRange(await GetHistoryAddresses(), new Position(latitude, longitude), 75);
            return found;
        }

        private Address GetAddressInRange (IEnumerable<Address> addresses, Position position, float range)
        {
            var addressesInRange = from a in addresses
                let distance = position.DistanceTo (new Position (a.Latitude, a.Longitude))
                    where distance <= range
                    orderby distance ascending
                    select a;
            
            return addressesInRange.FirstOrDefault ();
        }
        
        public Account CurrentAccount 
		{
            get 
			{
                var account = UserCache.Get<Account> ("LoggedUser");
                return account;
            }
            private set 
			{
                if (value != null) 
				{
                    UserCache.Set ("LoggedUser", value);    
                } 
				else 
				{
                    UserCache.Clear ("LoggedUser");
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
				DefaultTipPercent = tipPercent,
				AccountNumber = settings.AccountNumber
            };
            
			UseServiceClientTask<IAccountServiceClient> (service => {                     
				return service.UpdateBookingSettings (bsr);                
            });

            var account = CurrentAccount;
            account.Settings = settings;
            account.DefaultCreditCard = creditCardId;
            account.DefaultTipPercent = tipPercent;

            //Set to update the cache
            CurrentAccount = account;
        }

		public void UpdateAccountNumber (string accountNumber)
		{
			var settings = CurrentAccount.Settings;
			settings.AccountNumber = accountNumber;
			UpdateSettings (settings, CurrentAccount.DefaultCreditCard, CurrentAccount.DefaultTipPercent);
		}

        public string UpdatePassword (Guid accountId, string currentPassword, string newPassword)
        {
			return UseServiceClientTask<IAccountServiceClient, string> (service => service.UpdatePassword (new UpdatePassword{ AccountId = accountId, CurrentPassword = currentPassword, NewPassword = newPassword }));
        }

		public async Task<Account> SignIn (string email, string password)
        {
			Logger.LogMessage("SignIn with server {0}", _appSettings.Data.ServiceUrl);
            try 
			{
				var authResponse = await UseServiceClientAsync<IAuthServiceClient, AuthenticationData>(service => service
					.Authenticate (email, password),
					error => { throw error; /* Avoid trigerring global error handler */ });
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
                switch (e.StatusCode)
                {
					case (int)HttpStatusCode.Unauthorized:
					{
						if (e.Message == AuthFailure.AccountNotActivated.ToString ())
						{
							throw new AuthException ("Account not validated", AuthFailure.AccountNotActivated, e);
						}
						else
						{
							throw new AuthException ("Invalid username or password", AuthFailure.InvalidUsernameOrPassword, e);
						}
					}
					case (int)HttpStatusCode.NotFound:
					{
						throw new AuthException ("Invalid service url", AuthFailure.InvalidServiceUrl, e);
					}
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
			Mvx.Resolve<ICacheService>().Set (AuthenticationDataCacheKey, authResponse);
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
                UserCache.Clear (HistoryAddressesCacheKey);
                UserCache.Clear (FavoriteAddressesCacheKey);

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
            try 
			{
                var account = UseServiceClientTask<IAccountServiceClient, Account>(service => service.GetMyAccount());
                CurrentAccount = account;
                return account;
            } 
			catch 
			{
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
			data.Language = _localize.CurrentLanguage;
			await UseServiceClientAsync<IAccountServiceClient> (service =>  service.RegisterAccount (data)); 
        }

        public void DeleteFavoriteAddress (Guid addressId)
        {
            if (addressId.HasValue()) 
			{
                var toDelete = addressId;
                
                RemoveFromCacheArray<Address> (FavoriteAddressesCacheKey, toDelete, (id, a) => a.Id == id);                

				UseServiceClientTask<IAccountServiceClient> (service => service.RemoveFavoriteAddress (toDelete));
            }
        }

        public void DeleteHistoryAddress (Guid addressId)
        {
            if (addressId.HasValue ()) 
			{
                var toDelete = addressId;

                RemoveFromCacheArray<Address> (HistoryAddressesCacheKey, toDelete, (id, a) => a.Id == id);

				UseServiceClientTask<IAccountServiceClient> (service => service.RemoveAddress (toDelete));
            }
        }

        public void UpdateAddress (Address address)
        {
            bool isNew = address.Id.IsNullOrEmpty ();
            if (isNew) 
			{
                address.Id = Guid.NewGuid ();
            }

            if (address.IsHistoric) 
			{
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

                if (isNew) 
				{                        
				  return service.AddFavoriteAddress (toSave);
                } 
				else if (address.IsHistoric) 
				{
				  return service.UpdateFavoriteAddress (toMove);
                } 
				else 
				{
			      return service.UpdateFavoriteAddress (toSave);
                }
            });
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

		public async Task<IList<VehicleType>> GetVehiclesList ()
        {
			return await UseServiceClientAsync<IVehicleClient, VehicleType[]>(service => service.GetVehicleTypes());
        }

		public async Task<IList<ListItem>> GetPaymentsList ()
        {
			var refData = await GetReferenceData();

			if (!CurrentAccount.DefaultCreditCard.HasValue)
		    {
		        refData.PaymentsList.Remove(i => i.Id == ChargeTypes.CardOnFile.Id);
		    }

            return refData.PaymentsList;
        }

        public async Task<CreditCardDetails> GetCreditCard ()
        {
			// the server can return multiple credit cards if the user added more cards with a previous version, we get the first one only.
            var result = await UseServiceClientAsync<IAccountServiceClient, IEnumerable<CreditCardDetails>>(service => service.GetCreditCards());
			return result.FirstOrDefault();
        }

		private async Task TokenizeCard(CreditCardInfos creditCard)
		{
			var response = await UseServiceClientAsync<IPaymentService, TokenizedCreditCardResponse>(service => service.Tokenize(
				creditCard.CardNumber, 
				new DateTime(creditCard.ExpirationYear.ToInt(), creditCard.ExpirationMonth.ToInt(), 1),
				creditCard.CCV));	

			creditCard.Token = response.CardOnFileToken;       
		}

		public async Task<bool> AddCreditCard (CreditCardInfos creditCard)
        {
			try
			{
				await TokenizeCard (creditCard);
			}
			catch
			{
				return false;
			}
            
            var request = new CreditCardRequest
            {
                CreditCardCompany = creditCard.CreditCardCompany,
                CreditCardId = creditCard.CreditCardId,
				NameOnCard = creditCard.NameOnCard,
                Last4Digits = creditCard.Last4Digits,
                Token = creditCard.Token,
				ExpirationMonth = creditCard.ExpirationMonth,
				ExpirationYear = creditCard.ExpirationYear
            };
            
			await UseServiceClientAsync<IAccountServiceClient> (client => client.AddCreditCard (request));  

			return true;
        }

		public async Task<bool> UpdateCreditCard(CreditCardInfos creditCard)
		{
			try
			{
				await TokenizeCard (creditCard);
			}
			catch
			{
				return false;
			}

			var request = new CreditCardRequest
			{
				CreditCardCompany = creditCard.CreditCardCompany,
				CreditCardId = creditCard.CreditCardId,
				NameOnCard = creditCard.NameOnCard,
				Last4Digits = creditCard.Last4Digits,
				Token = creditCard.Token,
				ExpirationMonth = creditCard.ExpirationMonth,
				ExpirationYear = creditCard.ExpirationYear
			};

			await UseServiceClientAsync<IAccountServiceClient> (client => client.UpdateCreditCard (request));  

			return true;
		}

		public async Task RemoveCreditCard()
		{
			await UseServiceClientAsync<IAccountServiceClient>(client => client.RemoveCreditCard());
		}

        public async Task<NotificationSettings> GetNotificationSettings(bool companyDefaultOnly = false, bool cleanCache = false)
        {
            var cachedSetting = companyDefaultOnly ? UserCache.Get<NotificationSettings>(CompanyNotificationSettingsCacheKey)
                                                   : UserCache.Get<NotificationSettings>(UserNotificationSettingsCacheKey);

            if (cachedSetting != null && !cleanCache)
            {
                return cachedSetting;
            }

            var companySettings = await UseServiceClientAsync<CompanyServiceClient, NotificationSettings>(client => client.GetNotificationSettings());
			UserCache.Set(CompanyNotificationSettingsCacheKey, companySettings);

			if (companyDefaultOnly)
            {
                return companySettings;
            }

            var userSettings = await UseServiceClientAsync<IAccountServiceClient, NotificationSettings>(client => client.GetNotificationSettings(CurrentAccount.Id));

            // Merge company and user settings together
            // If the value is not null in the company settings, this means the setting is active and visible to the user
            // we check if the user has a value otherwise we put the company default value (or null if set as "not available" by the company)
            var mergedSettings =  new NotificationSettings
                {
                    Id = userSettings.Id,
                    Enabled = companySettings.Enabled && userSettings.Enabled,
                    BookingConfirmationEmail = companySettings.BookingConfirmationEmail.HasValue && userSettings.BookingConfirmationEmail.HasValue
                        ? userSettings.BookingConfirmationEmail 
                        : companySettings.BookingConfirmationEmail,
                    ConfirmPairingPush = companySettings.ConfirmPairingPush.HasValue && userSettings.ConfirmPairingPush.HasValue
                        ? userSettings.ConfirmPairingPush 
                        : companySettings.ConfirmPairingPush,
                    DriverAssignedPush = companySettings.DriverAssignedPush.HasValue && userSettings.DriverAssignedPush.HasValue
                        ? userSettings.DriverAssignedPush 
                        : companySettings.DriverAssignedPush,
                    NearbyTaxiPush = companySettings.NearbyTaxiPush.HasValue && userSettings.NearbyTaxiPush.HasValue
                        ? userSettings.NearbyTaxiPush 
                        : companySettings.NearbyTaxiPush,
                    PaymentConfirmationPush = companySettings.PaymentConfirmationPush.HasValue && userSettings.PaymentConfirmationPush.HasValue
                        ? userSettings.PaymentConfirmationPush 
                        : companySettings.PaymentConfirmationPush,
                    ReceiptEmail = companySettings.ReceiptEmail.HasValue && userSettings.ReceiptEmail.HasValue
                        ? userSettings.ReceiptEmail 
                        : companySettings.ReceiptEmail,
                    VehicleAtPickupPush = companySettings.VehicleAtPickupPush.HasValue && userSettings.VehicleAtPickupPush.HasValue
                        ? userSettings.VehicleAtPickupPush 
                        : companySettings.VehicleAtPickupPush
                };

            UserCache.Set(UserNotificationSettingsCacheKey, mergedSettings);
            return mergedSettings;
        }

        public async Task UpdateNotificationSettings(NotificationSettings notificationSettings)
        {
            // Update cached user settings
            UserCache.Set(UserNotificationSettingsCacheKey, notificationSettings);

            var request = new NotificationSettingsRequest
            {
                AccountId = CurrentAccount.Id,
                NotificationSettings = notificationSettings
            };

            await UseServiceClientAsync<IAccountServiceClient>(client => client.UpdateNotificationSettings(request));
        }

		public void LogApplicationStartUp()
		{
			try
			{
				var packageInfo = Mvx.Resolve<IPackageInfo> ();

				var request = new LogApplicationStartUpRequest
				{
					StartUpDate = DateTime.UtcNow,
					Platform = packageInfo.Platform,
					PlatformDetails = packageInfo.PlatformDetails,
					ApplicationVersion = packageInfo.Version
				};

				// No need to await since we do not want to slowdown the app
				UseServiceClientAsync<IAccountServiceClient> (client => client.LogApplicationStartUp(request));
			}
			catch (Exception e)
			{
				// If logging fails, run app anyway and log exception
                Logger.LogError(e);
			}
		}
    }
}