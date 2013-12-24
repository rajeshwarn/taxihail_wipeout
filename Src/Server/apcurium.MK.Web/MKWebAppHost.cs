using System;
using apcurium.MK.Booking.Api.Serialization;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using log4net;
using Microsoft.Practices.Unity;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.Text;
using ServiceStack.Text.Common;
using ServiceStack.WebHost.Endpoints;
using Funq;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Validation;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Security;
using apcurium.MK.Common.IoC;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;
using ServiceStack.Common.Web;

namespace apcurium.MK.Web
{
    public class MkWebAppHost : AppHostBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MkWebAppHost));

        public MkWebAppHost()
            : base("Mobile Knowledge Web Services", typeof(CurrentAccountService).Assembly)
        {

            JsConfig.Reset();
            JsConfig.EmitCamelCaseNames = true;
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            JsConfig<DateTime?>.RawDeserializeFn = NullableDateTimeRawDesirializtion;


        }



        private DateTime? NullableDateTimeRawDesirializtion(string s)
        {
            try
            {
                if (s.IndexOf('.') > 0)
                {
                    s = s.Substring(0, s.IndexOf('.'));
                }
                return DateTimeSerializer.ParseShortestXsdDateTime(s);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override void Configure(Container containerFunq)
        {
            Log.Info("Configure AppHost");
            new Module().Init(UnityServiceLocator.Instance);

            var container = UnityServiceLocator.Instance;
            containerFunq.Adapter = new UnityContainerAdapter(container, new Logger());

            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[]
                    {
                        new CustomCredentialsAuthProvider(container.Resolve<ICommandBus>(), container.Resolve<IAccountDao>(), container.Resolve<IPasswordService>()),
                        new CustomFacebookAuthProvider(container.Resolve<IAccountDao>()), 
                        new CustomTwitterAuthProvider(container.Resolve<IAccountDao>()), 
                    }));

            Plugins.Add(new ValidationFeature());
            containerFunq.RegisterValidators(typeof(SaveFavoriteAddressValidator).Assembly);

            RequestFilters.Add((httpReq, httpResp, requestDto) =>
            {
                var authSession = httpReq.GetSession();
                if (authSession != null && authSession.UserAuthId != null)
                {
                    var account = container.Resolve<IAccountDao>().FindById(new Guid(authSession.UserAuthId));
                    if (account.DisabledByAdmin)
                    {
                        httpReq.RemoveSession();
                    }
                }
            });

            SetConfig(new EndpointHostConfig
            {
                GlobalResponseHeaders =
                        {
                            { "Access-Control-Allow-Origin", "*" },
                            { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
                        },
            });


            ContentTypeFilters.Register("text/x-csv", CsvCustomSerializer.SerializeToStream, CsvCustomSerializer.DeserializeFromStream);
            ResponseFilters.Add((req, res, dto) =>
            {
                if (req.ResponseContentType == "text/x-csv")
                {
                    res.AddHeader(HttpHeaders.ContentDisposition,
                        string.Format("attachment;filename={0}.csv", req.OperationName));
                }
            });

            Log.Info("Configure AppHost finished");
        }

    }
}