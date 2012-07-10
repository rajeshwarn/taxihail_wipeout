using System;
using System.Linq;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class AccountService : IAccountService
    {

        private static ReferenceData _refData;


        public AccountService()
        {

        }




        public void EnsureListLoaded()
        {

            if (_refData == null)
            {
                try
                {

                    var service = TinyIoCContainer.Current.Resolve<ReferenceDataServiceClient>();

                    _refData = service.GetReferenceData();
                }
                catch (Exception ex)
                {
                    
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);

                }
            }
        }


        


        protected ILogger Logger
        {
            get { return TinyIoCContainer.Current.Resolve<ILogger>(); }
        }

        public void ResendConfirmationEmail(string email)
        {

        }



        public LocationData[] GetHistoryAddresses()
        {
            //var historic = loggedUser.BookingHistory.Where(b => !b.Hide && b.PickupLocation.Name.IsNullOrEmpty()).OrderByDescending(b => b.RequestedDateTime).Select(b => b.PickupLocation).ToArray();
            return new LocationData[0];
        }

        public LocationData[] GetFavoriteAddresses()
        {
            //var historic = loggedUser.BookingHistory.Where(b => !b.Hide && b.PickupLocation.Name.IsNullOrEmpty()).OrderByDescending(b => b.RequestedDateTime).Select(b => b.PickupLocation).ToArray();
            return new LocationData[0];
        }


        public Address FindInAccountAddresses(double latitude, double longitude)
        {
            Address found = null;

            var service = TinyIoCContainer.Current.Resolve<AccountServiceClient>();
            var favorites = service.GetFavoriteAddresses(CurrentAccount.Id);
            
            if (favorites.Count > 0)
            {
                found = favorites.FirstOrDefault( a=> (Math.Abs(a.Longitude - longitude) <= 0.002) && (Math.Abs(a.Latitude - latitude) <= 0.002));                                
            }

            //if (found == null)
            //{
            //    var historics = service.GetHistorucAddresses();

            //    if (historics.Count > 0)
            //    {
            //        found = historics.FirstOrDefault(a => (Math.Abs(a.Longitude - longitude) <= 0.001) && (Math.Abs(a.Latitude - latitude) <= 0.001));
            //    }
            //}

            return found;

            //    var closeLocation =
            //        AppContext.Current.LoggedUser.FavoriteLocations.Where(
            //            d => d.Latitude.HasValue && d.Longitude.HasValue).FirstOrDefault(
            //                d =>
            //                (Math.Abs(d.Longitude.Value - addresses[0].Longitude) <= 0.002) &&
            //                (Math.Abs(d.Latitude.Value - addresses[0].Latitude) <= 0.002));
            //    if (closeLocation == null)
            //    {
            //        //closeLocation =
            //        //    AppContext.Current.LoggedUser.BookingHistory.Where(
            //        //        b =>
            //        //        !b.Hide && (b.PickupLocation != null) && b.PickupLocation.Latitude.HasValue &&
            //        //        b.PickupLocation.Longitude.HasValue).Select(b => b.PickupLocation).FirstOrDefault(
            //        //            d =>
            //        //            (Math.Abs(d.Longitude.Value - locations[0].Longitude.Value) <= 0.001) &&
            //        //            (Math.Abs(d.Latitude.Value - locations[0].Latitude.Value) <= 0.001));
            //    }

            //    if (closeLocation != null )
            //    {
            //        SetLocationData(closeLocation, changeZoom);

            //    }
            //    else if (locations.Any())
            //    {
            //        SetLocationData(addresses.First(), changeZoom);

            //    }
            //}
            

        }

        private Account CurrentAccount
        {
            get
            {
                return TinyIoCContainer.Current.Resolve<IAppContext>().LoggedUser;
            }
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
                var parameters = new NamedParameterOverloads(  );
                parameters.Add( "credential", new AuthInfo(email, password) ) ;

                var service = TinyIoCContainer.Current.Resolve<AccountServiceClient>( "Authenticate",  parameters );
                var account = service.GetMyAccount();
                if (account != null)
                {
                    context.UpdateLoggedInUser(account, false);

                    //TODO: Should not keep password like this
                    context.LoggedInPassword = password;
                    data = account;

                }

                EnsureListLoaded();

                
                isSuccess = true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                isSuccess = false;
            }




            return data;

            
        }


        public bool ResetPassword(ResetPasswordData data)
        {
            bool isSuccess = false;

            try
            {
                var service = TinyIoCContainer.Current.Resolve<AccountServiceClient>("NotAuthenticated");
                service.ResetPassword(data.Email);
                isSuccess = true;
            }
            catch( Exception ex )
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
            
            try
            {
                var service = TinyIoCContainer.Current.Resolve<AccountServiceClient>("NotAuthenticated");
                service.RegisterAccount(data);
                isSuccess = true;
            }
            catch( Exception ex )
            {
                lError = ex.Message;
                isSuccess = false;
            }

            error = lError;
            return isSuccess;
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


