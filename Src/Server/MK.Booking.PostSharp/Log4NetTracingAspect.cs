using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web.Services.Protocols;
using log4net;
using PostSharp.Aspects;
using ServiceStack.Text;

namespace MK.Booking.PostSharp
{
    [Serializable]
    [Log4NetTracingAspect(AttributeExclude = true)]
    public class Log4NetTracingAspect : OnMethodBoundaryAspect
    {
        private readonly Dictionary<Type, ILog> _loggers = new Dictionary<Type, ILog>();
        private readonly string _prefix;

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
                var declaringType = args.Method.DeclaringType;

                if (declaringType == null)
                {
                    return;
                }

                if (!_loggers.ContainsKey(declaringType))
                {
                    _loggers.Add(declaringType, LogManager.GetLogger(_prefix + declaringType.FullName));
                }

                var logger = _loggers[declaringType];

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
            
            logger.Info(string.Format("Call made to : {0} executed in {1}ms with parameters {2}", args.Method.Name, executionTime, parameters));
                       
            string returnedValue = null;
            if (args.ReturnValue != null)
            {
                returnedValue = args.ReturnValue.ToJson();
            }
            logger.Info(string.Format("Call made to : {0} executed in {1}ms with result {2}", args.Method.Name, executionTime, returnedValue ?? "No result"));
        }
    }
}

