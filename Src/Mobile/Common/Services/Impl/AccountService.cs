using System;
using System.Diagnostics;

using System.Linq;
using System.ServiceModel;
using apcurium.Framework;
using apcurium.Framework.Extensions;
using Microsoft.Practices.ServiceLocation;
using MobileTaxiApp.Infrastructure;
using IBS = TaxiMobileApp.Lib.IBS;
using System.Collections.Generic;
using TaxiMobileApp.Lib.IBS;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading;

namespace TaxiMobileApp
{
    public class AccountService : IAccountService
    {
        public void UseService(Action<TaxiMobileApp.Lib.IBS.AccountService> action)
        {
            var serviceUrl = ServiceLocator.Current.GetInstance<IAppSettings>().ServiceUrl;

            var service = new IBS.AccountService(serviceUrl + "AccountService.asmx");



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

                        if (result.Error == IBS.ErrorCode.NoError)
                        {
                            //_companies.ForEach ( c=> Console.WriteLine( c.Id.ToString () + "-" + c.Display ) );
                            _companies = result.Companies.Where(c => validCompaniesId.Any(v => v == c.Id)).Select(c => new ListItem { Id = c.Id, Display = c.Name }).ToArray();
                        }


                        var resultV = service.GetVehiclesList(sessionId);

                        if (resultV.Error == IBS.ErrorCode.NoError)
                        {
                            _vehicules = resultV.Vehicles.Select(c => new ListItem { Id = Convert.ToInt32(c.Id), Display = c.Description }).ToArray();
                        }


                        var resultP = service.GetChargeTypesList(sessionId, ServiceLocator.Current.GetInstance<IAppResource>().CurrentLanguage == AppLanguage.English ? "en" : "fr");

                        if (resultP.Error == IBS.ErrorCode.NoError)
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

        
        public AccountService()
        {
        }


        protected ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

		public void ResendConfirmationEmail(string email)
		{
			UseService(service => {
//				service.ResendConfirmationEmail( email );
			});
		}

        public AccountData GetAccount(string email, string password, out string error, out ErrorCode errorCode )
        {
            error = "";
            string resultError = "";
			ErrorCode resultErrorCode = ErrorCode.NoError;
            EnsureListLoaded();

            AccountData data = null;
            UseService(service =>
            {

                try
                {
                    var sessionId = service.Authenticate("iphone", "test", 1);


//                    var re = service.GetRideExceptionsList(sessionId, "EN");
//                    re.RideExceptions.ForEach(rrr => Console.WriteLine(rrr.Description));


                    Logger.StartStopwatch("WS GetAccount : " + email.ToLower());

                    var account = service.GetAccount(sessionId, email.ToLower(), password);

                    Logger.StopStopwatch("WS GetAccount : " + email.ToLower());


                    var result = new AccountData();
					
                    var loggedUser = ServiceLocator.Current.GetInstance<IAppContext>().LoggedUser;

                    if ((account.Error == IBS.ErrorCode.NoError) && (account.Account != null))
                    {
                        result = new AccountMapping().ToData(loggedUser, account.Account);
                       
                        var history = service.GetOrderHistoryEx(sessionId, email, password, DateTime.Now.AddMonths(-1), DateTime.Now);
                        var orders = new List<OrderInfo>();
                        if ((history.Error == IBS.ErrorCode.NoError) && (history.OrderInfos != null))
                        {
                            orders.AddRange(history.OrderInfos);
                        }

                        var orderExisting = service.GetOrdersList(sessionId, email, password);
                        if ((orderExisting.Error == IBS.ErrorCode.NoError) && (orderExisting.OrderInfos != null))
                        {
                            orders.AddRange(orderExisting.OrderInfos);
                        }

                        if (orders.Count > 0)
                        {
                            new OrderMapping().UpdateHistory(result, orders.ToArray(), _vehicules, _companies, _payments);
                        }

                        result.Password = password;
                        new SettingMapper().SetSetting(result, account.Account);
						
						if ( result.DefaultSettings.Company != 12 )
						{
							result.DefaultSettings.Company = 12;
							result.DefaultSettings.CompanyName = GetCompaniesList ().Single ( c=>c.Id == 12).Display;
						}
                        data = result;
                    }

                    else
                    {
                        resultError = account.ErrorMessage;
						resultErrorCode = account.Error;

                    }
                }
                catch (Exception ex)
                {
                    ServiceLocator.Current.GetInstance<ILogger>().LogError(ex);
                }
            });
            error = resultError;
			errorCode = resultErrorCode;
			return data;
        }


        public bool ResetPassword(ResetPasswordData data)
        {
            bool isSuccess = false;
            UseService(service =>
            {
                var sessionId = service.Authenticate("iphone", "test", 1);
                var result = service.ResetPassword(sessionId, data.Email);
                if (result.Error == IBS.ErrorCode.NoError)
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
            bool isSuccess = true;
            string lError = "";

            //var service = new AccountServiceClient(@"http://192.168.12.116/apcurium.MK.Web/api/", null);
            var service = new AccountServiceClient(@"http://project.apcurium.com/apcurium.MK.Web.csproj_deploy/api/", null);
            service.RegisterAccount(new RegisterAccount { AccountId = Guid.NewGuid(), Email = data.Email, FirstName = data.FirstName, LastName = data.LastName, Password = data.Password, Phone = data.Mobile });

            Thread.Sleep( 2000 );
            var service2 = new AccountServiceClient(@"http://project.apcurium.com/apcurium.MK.Web.csproj_deploy/api/", new AuthInfo(data.Email, data.Password));
            var acc = service2.GetMyAccount();



            Console.WriteLine( acc.Email );

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
            UseService(service =>
            {
                Logger.LogMessage("Update user");

                var sessionId = service.Authenticate("iphone", "test", 1);

                var account = service.GetAccount(sessionId, data.Email, data.Password);

                if (account.Error == IBS.ErrorCode.NoError)
                {
                    Logger.LogMessage("Update user : No error");
                    var toUpdate = new AccountMapping().ToWSData(account.Account, data);
                    new SettingMapper().SetWSSetting(toUpdate, data);
                    toUpdate.Password = data.Password;


                    var result = service.UpdateAccount(sessionId, toUpdate);
                    if (result.Error != IBS.ErrorCode.NoError)
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


