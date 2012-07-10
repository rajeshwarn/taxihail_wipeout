using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Common.Entity;
using Microsoft.Practices.ServiceLocation;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;

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
                    ServiceLocator.Current.GetInstance<ILogger>().LogError(ex);

                }
            }
        }


        


        protected ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
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

        public AccountData GetAccount(string email, string password, out string error)
        {
            error = "";
            string resultError = "";

            EnsureListLoaded();

            AccountData data = null;
            //UseService(service =>
            //{

            //    try
            //    {
            //        var sessionId = service.Authenticate("iphone", "test", 1);


            //        //                    var re = service.GetRideExceptionsList(sessionId, "EN");
            //        //                    re.RideExceptions.ForEach(rrr => Console.WriteLine(rrr.Description));


            //        Logger.StartStopwatch("WS GetAccount : " + email.ToLower());

            //        var account = service.GetAccount(sessionId, email.ToLower(), password);

            //        Logger.StopStopwatch("WS GetAccount : " + email.ToLower());


            //        var result = new AccountData();

            //        var loggedUser = ServiceLocator.Current.GetInstance<IAppContext>().LoggedUser;

            //        if ((account.Error == IBS.ErrorCode.NoError) && (account.Account != null))
            //        {
            //            result = new AccountMapping().ToData(loggedUser, account.Account);

            //            var history = service.GetOrderHistoryEx(sessionId, email, password, DateTime.Now.AddMonths(-1), DateTime.Now);
            //            var orders = new List<OrderInfo>();
            //            if ((history.Error == IBS.ErrorCode.NoError) && (history.OrderInfos != null))
            //            {
            //                orders.AddRange(history.OrderInfos);
            //            }

            //            var orderExisting = service.GetOrdersList(sessionId, email, password);
            //            if ((orderExisting.Error == IBS.ErrorCode.NoError) && (orderExisting.OrderInfos != null))
            //            {
            //                orders.AddRange(orderExisting.OrderInfos);
            //            }

            //            if (orders.Count > 0)
            //            {
            //                new OrderMapping().UpdateHistory(result, orders.ToArray(), _vehicules, _companies, _payments);
            //            }

            //            result.Password = password;
            //            new SettingMapper().SetSetting(result, account.Account);

            //            if (result.DefaultSettings.Company != 12)
            //            {
            //                result.DefaultSettings.Company = 12;
            //                result.DefaultSettings.CompanyName = GetCompaniesList().Single(c => c.Id == 12).Display;
            //            }
            //            data = result;
            //        }

            //        else
            //        {
            //            resultError = account.ErrorMessage;
            //            resultErrorCode = account.Error;

            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        ServiceLocator.Current.GetInstance<ILogger>().LogError(ex);
            //    }
            //});
            //error = resultError;
            return data;
        }


        public bool ResetPassword(ResetPasswordData data)
        {
            bool isSuccess = false;
            //UseService(service =>
            //{
            //    var sessionId = service.Authenticate("iphone", "test", 1);
            //    var result = service.ResetPassword(sessionId, data.Email);
            //    if (result.Error == IBS.ErrorCode.NoError)
            //    {
            //        isSuccess = true;
            //    }
            //    else
            //    {
            //        Logger.LogMessage("ResetPassword : Error : " + result.Error.ToString() + " - " + result.ErrorMessage.ToSafeString());
            //    }
            //});

            return isSuccess;
        }

        public bool CreateAccount(CreateAccountData data, out string error)
        {
            bool isSuccess = true;
            string lError = "";

            //var service = new AccountServiceClient(@, null);
            var service =TinyIoCContainer.Current.Resolve<AccountServiceClient>();
            service.RegisterAccount(new RegisterAccount { AccountId = Guid.NewGuid(), Email = data.Email, FirstName = data.FirstName, LastName = data.LastName, Password = data.Password, Phone = data.Mobile });


            //var service2 = new AccountServiceClient(@"http://project.apcurium.com/apcurium.MK.Web.csproj_deploy/api/", new AuthInfo(data.Email, data.Password));
            //var acc = service2.GetMyAccount();



            //UseService(service =>
            //{
            //    var sessionId = service.Authenticate("iphone", "test", 1);
            //    var account = new IBS.AccountInfo();
            //    account.Email = data.Email;
            //    account.Title = data.Title;
            //    account.FirstName = data.FirstName;
            //    account.LastName = data.LastName;
            //    account.PhoneNumber = data.Phone;
            //    account.MobileNumber = data.Mobile;
            //    account.Language = ServiceLocator.Current.GetInstance<IAppResource>().CurrentLanguage == AppLanguage.English ? "E" : "F";
            //    account.Password = data.Password;

            //    var result = service.CreateAccount(sessionId, account);
            //    if (result.Error == IBS.ErrorCode.NoError)
            //    {
            //        isSuccess = true;
            //    }
            //    else
            //    {
            //        lError = result.ErrorMessage.ToSafeString();
            //        Logger.LogMessage("ResetPassword : Error : " + result.Error.ToString() + " - " + result.ErrorMessage.ToSafeString());
            //    }
            //});
            error = lError;
            return isSuccess;
        }


        public AccountData UpdateUser(AccountData data)
        {
            AccountData r = null;
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
            //            var loggedUser = ServiceLocator.Current.GetInstance<IAppContext>().LoggedUser;
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


