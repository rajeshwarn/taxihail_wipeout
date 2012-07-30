using System;
using System.Linq;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
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

        public AccountService()
        {

        }

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

        public void RefreshCache()
        {
            TinyIoCContainer.Current.Resolve<ICacheService>().Clear(_historyAddressesCacheKey);
            GetHistoryAddresses();
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
                result = service.GetOrders(CurrentAccount.Id);
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
                    result = service.GetFavoriteAddresses(CurrentAccount.Id);
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

        public void UpdateSettings(BookingSettings settings)
        {
            QueueCommand<AccountServiceClient>(service =>
            {                     
                service.UpdateBookingSettings(CurrentAccount.Id, new BookingSettingsRequest{ Name =  settings.Name, Phone=settings.Phone, Passengers = settings.Passengers, VehicleTypeId = settings.VehicleTypeId, ChargeTypeId = settings.ChargeTypeId, ProviderId = settings.ProviderId });
                CurrentAccount.Settings = settings;
            });

        }

        public Account GetAccount(string email, string password, out string error)
        {
            error = "";
            string resultError = "";
            bool isSuccess = false;
            Account data = null;

            try
            {

                var context = TinyIoCContainer.Current.Resolve<IAppContext>();
                var parameters = new NamedParameterOverloads();
                parameters.Add("credential", new AuthInfo(email, password));


                var service = TinyIoCContainer.Current.Resolve<AccountServiceClient>("Authenticate", parameters);
                var account = service.GetMyAccount();
                if (account != null)
                {
                    context.UpdateLoggedInUser(account, false);
                    context.LoggedInEmail = email;
                    context.LoggedInPassword = password;
                    data = account;
                }
                else
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("***************AUTH FAILED ");
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

            try
            {
                var service = TinyIoCContainer.Current.Resolve<AccountServiceClient>("NotAuthenticated");
                service.RegisterAccount(data);
                isSuccess = true;
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
                    service.RemoveFavoriteAddress(accountId, toDelete);
                }
                );
            }
            

        }

        public void UpdateAddress(Address address)
        {
            bool isNew = address.Id.IsNullOrEmpty();
            if (isNew)
            {
                address.Id = Guid.NewGuid();
            }

            UpdateCacheArray(_favoriteAddressesCacheKey, address, (a1, a2) => a1.Id.Equals(a2.Id));


            QueueCommand<AccountServiceClient>(service =>
            {
                var toSave = new SaveFavoriteAddress
                    {
                        AccountId = CurrentAccount.Id,
                        Apartment = address.Apartment,
                        FriendlyName = address.FriendlyName,
                        FullAddress = address.FullAddress,
                        Id = address.Id,
                        Latitude = address.Latitude,
                        Longitude = address.Longitude,
                        RingCode = address.RingCode
                    };

                if (isNew)
                {                        
                    service.AddFavoriteAddress(toSave);
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


