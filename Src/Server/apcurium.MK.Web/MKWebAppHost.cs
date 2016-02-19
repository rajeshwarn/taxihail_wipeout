#region

using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
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
using UnityContainerExtensions = Microsoft.Practices.Unity.UnityContainerExtensions;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.App_Start;
using Microsoft.Owin;
using Owin;
#endregion


namespace apcurium.MK.Web
{
    public partial class MkWebAppHost 
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MkWebAppHost));

        public void Configuration(IAppBuilder app)
        {
            

            

            //ConfigureAuth(app);

            
        }

        public void Configure(Container containerFunq)
        {
    //        
    //        

    //        
    //        containerFunq.Adapter = new UnityContainerAdapter(container, new Logger());
            

    //        Plugins.Add(new AuthFeature(() => new AuthUserSession(),
    //            new IAuthProvider[]
    //            {
    //                new CustomCredentialsAuthProvider(UnityContainerExtensions.Resolve<ICommandBus>(container),
    //                    UnityContainerExtensions.Resolve<IAccountDao>(container),
    //                    UnityContainerExtensions.Resolve<IPasswordService>(container),
    //                    UnityContainerExtensions.Resolve<IServerSettings>(container)),
    //                new CustomFacebookAuthProvider(UnityContainerExtensions.Resolve<IAccountDao>(container)),
    //                new CustomTwitterAuthProvider(UnityContainerExtensions.Resolve<IAccountDao>(container))
    //            }));

    //        Plugins.Add(new ValidationFeature());
    //        containerFunq.RegisterValidators(typeof (SaveFavoriteAddressValidator).Assembly);

    //        RequestFilters.Add((httpReq, httpResp, requestDto) =>
    //        {
    //            var authSession = httpReq.GetSession();
    //            if (authSession != null && !string.IsNullOrEmpty(authSession.UserAuthId))
    //            {
    //                var account = container.Resolve<IAccountDao>().FindById(new Guid(authSession.UserAuthId));
    //                if (account == null || account.DisabledByAdmin)
    //                {
    //                    httpReq.RemoveSession();
    //                }
    //            }
    //        });

    //        SetConfig(new EndpointHostConfig
    //        {
    //            GlobalResponseHeaders =
    //            {
    //                {"Access-Control-Allow-Origin", "*"},
    //                {"Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS"},
    //            },

				//EnableFeatures = Feature.All.Remove(Feature.Metadata)
    //        });


    //        ContentTypeFilters.Register("text/x-csv", CsvCustomSerializer.SerializeToStream, CsvCustomSerializer.DeserializeFromStream);
    //        ResponseFilters.Add((req, res, dto) =>
    //        {
    //            if (req.ResponseContentType == "text/x-csv")
    //            {
    //                res.AddHeader(HttpHeaders.ContentDisposition,
    //                    string.Format("attachment;filename={0}.csv", req.OperationName));
    //            }
    //        });

    //        Log.Info("Configure AppHost finished");
        }
    }
}