using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class AppContext // : IAppContext
    {
        private static AppContext _current;

        public static void Initialize(UIWindow window)
        {
            _current = new AppContext(window);
        }

        public static AppContext Current
        {
            get { return _current; }
        }
        
        private UINavigationController _controller;


        private AppContext(UIWindow window)
        {
            Window = window;
        }

        public UIWindow Window  { get; private set; }
            
        
		public UINavigationController Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }
     
        
    }
	
    
}


