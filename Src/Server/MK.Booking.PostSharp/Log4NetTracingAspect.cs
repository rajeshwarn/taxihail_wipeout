using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using log4net;
using log4net.Core;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using ServiceStack.Text;

namespace MK.Booking.PostSharp
{
    [Serializable]
    [Log4NetTracingAspect(AttributeExclude = true)]
    public class Log4NetTracingAspect : OnMethodBoundaryAspect
    {

        private Dictionary<Type, ILog> _loggers = new Dictionary<Type, ILog>();
        private string _prefix;

        public Log4NetTracingAspect()
        {
        }

        public Log4NetTracingAspect(string prefix)
        {
            _prefix = prefix;
        }


        public override void OnEntry(MethodExecutionArgs args)
        {
            var watch = new Stopwatch();
            watch.Start();
            args.MethodExecutionTag = watch;
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            if ((!args.Method.IsConstructor) && (args.Method.MemberType == MemberTypes.Method) && (!args.Method.IsSpecialName))
            {
                if (!_loggers.ContainsKey(args.Method.DeclaringType))
                {
                    _loggers.Add(args.Method.DeclaringType, LogManager.GetLogger(_prefix + args.Method.DeclaringType.FullName));
                }

                ILog logger = _loggers[args.Method.DeclaringType];



                long executionTime = 0;
                if (args.MethodExecutionTag is Stopwatch)
                {
                    var watch = (Stopwatch)args.MethodExecutionTag;
                    executionTime = watch.ElapsedMilliseconds;
                }

                LogInfo(logger, args, executionTime);


                if (args.Exception != null)
                {
                    logger.Error("Critical Error", args.Exception);
                }

            }
        }

        private void LogInfo(ILog logger, MethodExecutionArgs args, long executionTime)
        {

            string parameters = "";
            
            foreach (var arg in args.Arguments)
            {
                parameters += string.Format( " [{0}] -", arg.ToJson());
            }

            if (args.Instance is SoapHttpClientProtocol)
            {
                logger.Info("Soap call to url : " + ((SoapHttpClientProtocol) args.Instance).Url);
            }
            
            logger.Info(string.Format("Call made to : {0} executed in {1}ms with parameters {2}", args.Method.Name,
             executionTime, parameters));


                       
            string returnedValue = null;
            if (args.ReturnValue != null)
            {
                returnedValue = args.ReturnValue.ToJson();
            }
            logger.Info(string.Format("Call made to : {0} executed in {1}ms with result {2}", args.Method.Name,
                executionTime, returnedValue ?? "No result"));

         


        }


    }


}

