//using System;
//using System.Linq;
//using apcurium.Framework.Extensions;
//using System.Collections.Generic;
//using apcurium.MK.Booking.Mobile.Practices;
//using apcurium.MK.Booking.Mobile.AppServices;

//namespace apcurium.MK.Booking.Mobile
//{
//    public class Bootstrapper
//    {
		
//        private IModule[] _modules;
		
//        public Bootstrapper (IModule[] modules)
//        {			
			
//            var list = new List<IModule>();
//            if ( modules.Count() > 0 )
//            {
//                list.AddRange( modules );
//            }
//            list.Add( new ModuleService() );
//             _modules = list.ToArray();
//        }
		
//        public void Run()
//        {												
//            _modules.ForEach(  m => m.Initialize() );
			
//        }
//    }
//}

