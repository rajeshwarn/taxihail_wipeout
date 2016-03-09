#region

using Funq;
using log4net;
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




            //containerFunq.Adapter = new UnityContainerAdapter(container, new Logger());


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

                    //SetConfig(new EndpointHostConfig
                    //{
                    //    GlobalResponseHeaders =
                    //    {
                    //        {"Access-Control-Allow-Origin", "*"},
                    //        {"Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS"},
                    //    },

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