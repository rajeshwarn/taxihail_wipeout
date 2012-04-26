using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using MobileTaxiApp.Infrastructure;
using TaxiMobile.Lib.IBS;
using apcurium.Framework.Extensions;
#if MONO_DROID
using Android.Runtime;
using apcurium.Framework.Extensions;

#endif
#if MONO_TOUCH
using MonoTouch.Foundation;
#endif

namespace TaxiMobileApp
{
    public class AccountService : IAccountService
    {


        public void UseService(Action<TaxiMobile.Lib.IBS.AccountService> action)
        {
            var serviceUrl = ServiceLocator.Current.GetInstance<IAppSettings>().ServiceUrl;

            var service = new TaxiMobile.Lib.IBS.AccountService(); //serviceUrl + "AccountService.asmx"



            try
            {
                action(service);
            }
            finally
            {
                //if (service.State == ConnectionState.Open) cnnX.Close();    // <<<<--- New addition

                service.Abort();
                service.Dispose();
                service = null;
            }


        }


        private static ListItem[] _companies;
        private static ListItem[] _payments;
        private static ListItem[] _vehicules;


        public void EnsureListLoaded()
        {

            if ((_companies == null) || (_vehicules == null) || (_payments == null))
            {


                UseService(service =>
                {
                    try
                    {

                        //						12-Taxi Diamond
                        //9-Taxi Diamond de L'Ouest
                        //10-Taxi Royal
                        //11-Taxi Candare
                        //14-TEST
                        //15-Taxi Diamond - Transport Adapté
                        //17-Taxi Diamond - Services Spécialisées
                        //18-***INTERNE*** Dorval
                        //19-CO-OP Taxi de Terrebonne

                        int[] validCompaniesId = new int[] { 12, 9, 10, 11, 14 };

                        var sessionId = service.Authenticate("iphone", "test", 1);
                        var result = service.GetCompaniesList(sessionId);

                        if (result.Error == ErrorCode.NoError)
                        {
                            //_companies.ForEach ( c=> Console.WriteLine( c.Id.ToString () + "-" + c.Display ) );
                            _companies = result.Companies.Where(c => validCompaniesId.Any(v => v == c.Id)).Select(c => new ListItem { Id = c.Id, Display = c.Name }).ToArray();
                        }


                        var resultV = service.GetVehiclesList(sessionId);

                        if (resultV.Error == ErrorCode.NoError)
                        {
                            _vehicules = resultV.Vehicles.Select(c => new ListItem { Id = Convert.ToInt32(c.Id), Display = c.Description }).ToArray();
                        }


                        var resultP = service.GetChargeTypesList(sessionId, ServiceLocator.Current.GetInstance<IAppResource>().CurrentLanguage == AppLanguage.English ? "en" : "fr");

                        if (resultP.Error == ErrorCode.NoError)
                        {
                            _payments = resultP.ChargeTypes.Select(c => new ListItem { Id = Convert.ToInt32(c.Id), Display = c.Description }).ToArray();
                        }


                    }
                    catch (Exception ex)
                    {
                        ServiceLocator.Current.GetInstance<ILogger>().LogError(ex);

                    }

                });

            }
        }

        [Preserve]
        public AccountService()
        {
        }


        protected ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public AccountData GetAccount(string email, string password, out string error)
        {
            error = "";
            string resultError = "";
            EnsureListLoaded();

            AccountData data = null;
            UseService(service =>
            {

                try
                {
                    var sessionId = service.Authenticate("iphone", "test", 1);


                    var re = service.GetRideExceptionsList(sessionId, "EN");
                    re.RideExceptions.ForEach(rrr => Console.WriteLine(rrr.Description));


                    Logger.StartStopwatch("WS GetAccount : " + email.ToLower());

                    var account = service.GetAccount(sessionId, email.ToLower(), password);

                    Logger.StopStopwatch("WS GetAccount : " + email.ToLower());


                    var result = new AccountData();

                    var loggedUser = ServiceLocator.Current.GetInstance<IAppContext>().LoggedUser;

                    if ((account.Error == ErrorCode.NoError) && (account.Account != null))
                    {
                        result = new AccountMapping().ToData(loggedUser, account.Account);
                        
                        var history = service.GetOrderHistoryEx(sessionId, email, password, DateTime.Now.AddMonths(-1), DateTime.Now);
                        var orders = new List<OrderInfo>();
                        if ((history.Error == ErrorCode.NoError) && (history.OrderInfos != null))
                        {
                            orders.AddRange(history.OrderInfos);
                        }

                        var orderExisting = service.GetOrdersList(sessionId, email, password);
                        if ((orderExisting.Error == ErrorCode.NoError) && (orderExisting.OrderInfos != null))
                        {
                            orders.AddRange(orderExisting.OrderInfos);
                        }

                        if (orders.Count > 0)
                        {
                            new OrderMapping().UpdateHistory(result, orders.ToArray(), _vehicules, _companies, _payments);
                        }

                        result.Password = password;
                        new SettingMapper().SetSetting(result, account.Account);
                        data = result;
                    }

                    else
                    {
                        resultError = account.ErrorMessage;

                    }
                }
                catch (Exception ex)
                {
                    ServiceLocator.Current.GetInstance<ILogger>().LogError(ex);
                }
            });

            error = resultError;
            return data;
        }


        public bool ResetPassword(ResetPasswordData data)
        {
            bool isSuccess = false;
            UseService(service =>
            {
                var sessionId = service.Authenticate("iphone", "test", 1);
                var result = service.ResetPassword(sessionId, data.Email);
                if (result.Error == ErrorCode.NoError)
                {
                    isSuccess = true;
                }
                else
                {
                    Logger.LogMessage("ResetPassword : Error : " + result.Error.ToString() + " - " + result.ErrorMessage.ToSafeString());
                }
            });

            return isSuccess;
        }

        public bool CreateAccount(CreateAccountData data, out string error)
        {
            bool isSuccess = false;
            string lError = "";
            UseService(service =>
            {
                var sessionId = service.Authenticate("iphone", "test", 1);
                var account = new AccountInfo();
                account.Email = data.Email;
                account.Title = data.Title;
                account.FirstName = data.FirstName;
                account.LastName = data.LastName;
                account.PhoneNumber = data.Phone;
                account.MobileNumber = data.Mobile;
                account.Language = ServiceLocator.Current.GetInstance<IAppResource>().CurrentLanguage == AppLanguage.English ? "E" : "F";
                account.Password = data.Password;

                var result = service.CreateAccount(sessionId, account);
                if (result.Error == ErrorCode.NoError)
                {
                    isSuccess = true;
                }
                else
                {
                    lError = result.ErrorMessage.ToSafeString();
                    Logger.LogMessage("ResetPassword : Error : " + result.Error.ToString() + " - " + result.ErrorMessage.ToSafeString());
                }
            });
            error = lError;
            return isSuccess;
        }


        public AccountData UpdateUser(AccountData data)
        {
            AccountData r = null;
            UseService(service =>
            {
                Logger.LogMessage("Update user");

                var sessionId = service.Authenticate("iphone", "test", 1);

                var account = service.GetAccount(sessionId, data.Email, data.Password);

                if (account.Error == ErrorCode.NoError)
                {
                    Logger.LogMessage("Update user : No error");
                    var toUpdate = new AccountMapping().ToWSData(account.Account, data);
                    new SettingMapper().SetWSSetting(toUpdate, data);
                    toUpdate.Password = data.Password;


                    var result = service.UpdateAccount(sessionId, toUpdate);
                    if (result.Error != ErrorCode.NoError)
                    {
                        r = data;
                    }
                    else
                    {
                        var loggedUser = ServiceLocator.Current.GetInstance<IAppContext>().LoggedUser;
                        r = new AccountMapping().ToData(loggedUser, result.Account);
                    }
                }

                else
                {
                    Logger.LogMessage("Update user : Error : " + account.Error.ToString() + " - " + account.ErrorMessage.ToSafeString());
                }


            });
            return r;
        }


        public ListItem[] GetCompaniesList()
        {
            EnsureListLoaded();
            return _companies;
        }



        public ListItem[] GetVehiclesList()
        {
            EnsureListLoaded();
            return _vehicules;
        }

        public ListItem[] GetPaymentsList()
        {
            EnsureListLoaded();
            return _payments;
        }
    }
}


