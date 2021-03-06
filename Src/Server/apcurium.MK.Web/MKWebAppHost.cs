﻿#region

using System;
using System.Web;
using System.Web.Routing;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Security;
using apcurium.MK.Booking.Api.Serialization;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Validation;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.IoC;
using Funq;
using Infrastructure.Messaging;
using log4net;
using Microsoft.Practices.Unity;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.Text;
using ServiceStack.Text.Common;
using ServiceStack.WebHost.Endpoints;
using UnityContainerExtensions = Microsoft.Practices.Unity.UnityContainerExtensions;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;
using apcurium.MK.Common.Configuration;
using ServiceStack.Common;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Web
{
    public class MkWebAppHost : AppHostBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MkWebAppHost));

        public MkWebAppHost()
            : base("Mobile Knowledge Web Services", typeof (CurrentAccountService).Assembly)
        {
            JsConfig.Reset();
            JsConfig.EmitCamelCaseNames = true;
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            JsConfig<DateTime?>.RawDeserializeFn = NullableDateTimeRawDesirializtion;
            JsConfig.IncludeNullValues = true;

            // Ensuring that Guids always serialize with the hyphens
            JsConfig<Guid>.RawSerializeFn += guid => EnsureGuidAsString(guid.ToString("D"));
            JsConfig<Guid?>.RawSerializeFn += guid => guid.HasValue ? EnsureGuidAsString(guid.Value.ToString("D")): null;
        }

        private string EnsureGuidAsString(string value)
        {
            return "\"{0}\"".InvariantCultureFormat(value);
        }

        private DateTime? NullableDateTimeRawDesirializtion(string s)
        {
            if (s == null)
            {
                return null;
            }

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

            var serverSettings = container.Resolve<IServerSettings>();
            
            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[]
                {
                    new CustomCredentialsAuthProvider(UnityContainerExtensions.Resolve<ICommandBus>(container),
                        UnityContainerExtensions.Resolve<IAccountDao>(container),
                        UnityContainerExtensions.Resolve<IPasswordService>(container),
                        UnityContainerExtensions.Resolve<IServerSettings>(container)),
                    new CustomFacebookAuthProvider(UnityContainerExtensions.Resolve<IAccountDao>(container)),
                    new CustomTwitterAuthProvider(UnityContainerExtensions.Resolve<IAccountDao>(container))
                }));

            Plugins.Add(new ValidationFeature());
            containerFunq.RegisterValidators(typeof (SaveFavoriteAddressValidator).Assembly);

            RequestFilters.Add((httpReq, httpResp, requestDto) =>
            {
                var authSession = httpReq.GetSession();
                if (authSession != null && !string.IsNullOrEmpty(authSession.UserAuthId))
                {
                    var account = container.Resolve<IAccountDao>().FindById(new Guid(authSession.UserAuthId));
                    if (account == null || account.DisabledByAdmin)
                    {
                        httpReq.RemoveSession();
                    }
                }
            });

            
            SetConfig(new EndpointHostConfig
            {
                GlobalResponseHeaders =
                {
                    {"Access-Control-Allow-Origin", "*"},
                    {"Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS"},
                },

                EnableFeatures = (serverSettings.ServerData.ShowAPIMetadata ? Feature.All : Feature.All.Remove(Feature.Metadata) )
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