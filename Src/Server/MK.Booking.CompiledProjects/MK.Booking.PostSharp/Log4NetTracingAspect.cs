using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace MK.Booking.PostSharp
{
    [Serializable]
    [Log4NetTracingAspect(AttributeExclude = true)]
    public class Log4NetTracingAspect: OnMethodBoundaryAspect
    {

        private Dictionary<Type, ILog> _loggers = new Dictionary<Type, ILog>();
        private string _prefix;

        public Log4NetTracingAspect() 
        {
        }

        public Log4NetTracingAspect( string prefix )
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
            if ( (!args.Method.IsConstructor) && (args.Method.MemberType == MemberTypes.Method) && ( !args.Method.IsSpecialName ))
            {
                if (!_loggers.ContainsKey(args.Method.DeclaringType))
                {
                    _loggers.Add(args.Method.DeclaringType, LogManager.GetLogger(_prefix + args.Method.DeclaringType.FullName));
                }
                ILog logger = _loggers[args.Method.DeclaringType];

                long executionTime = 0;
                if (args.MethodExecutionTag is Stopwatch)
                {
                    var watch = (Stopwatch) args.MethodExecutionTag;
                    executionTime = watch.ElapsedMilliseconds;
                    
                }
                logger.Info(args.Method.Name + " in " + executionTime.ToString() + "ms");
                if (args.Exception!=null)
                {
                    logger.Error("Critical Error", args.Exception );
                }

                

                
                //Trace.WriteLine(args.Method.Name + " in " + executionTime.ToString() + "ms", "IBS");
                //Console.WriteLine(args.Method.Name + " in " + executionTime.ToString() + "ms");
            }
        }


    }


}

