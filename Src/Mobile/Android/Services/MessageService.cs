//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
//namespace apcurium.MK.Booking.Mobile.Client.Services
//{
//    [Service(Label = "MessageService")]				
//    public class MessageHandlingService: Service
//    {
//        private BroadcastReceiver _messageReceiver;
//        public override void OnCreate()
//        {
//            base.OnCreate();

//            IntentFilter filter = new IntentFilter(MessageService.ACTION_SERVICE_MESSAGE);
//            filter.AddCategory(Intent.CategoryDefault);
//            _messageReceiver = new MessageBroadcastReceiver(this);
//            RegisterReceiver(_messageReceiver, filter);
//        }

//        public override IBinder OnBind(Intent intent)
//        {
//            return null;
//        }

//        public override void OnDestroy()
//        {
//            base.OnDestroy();
//            UnregisterReceiver(_messageReceiver);
//        }

//        public void DisplayMessage(Context context, Intent intent)
//        {
//            var title = Resources.GetString(Resource.String.ServiceErrorCallTitle);
//            var message = Resources.GetString(Resource.String.ServiceErrorDefaultMessage);
//            try
//            {
//                switch (intent.Action)
//                {
//                    case MessageService.ACTION_SERVICE_MESSAGE:
//                        var key = intent.GetStringExtra(MessageService.ACTION_EXTRA_MESSAGE);
//                        var key = intent.GetStringExtra(MessageService.ACTION_EXTRA_Title);
//                        message = Resources.GetString(identifier);
//                        break;
//                }
//            }
//            catch
//            {

//            }

//            var i = new Intent(this, typeof(AlertDialogActivity));
//            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
//            i.PutExtra("Title", title);
//            i.PutExtra("Message", message);
//            StartActivity(i);


//        }


//        [BroadcastReceiver]
//        public class MessageBroadcastReceiver : BroadcastReceiver
//        {
//            private MessageHandlingService _service;

//            public MessageBroadcastReceiver()
//            {
//            }

//            public MessageBroadcastReceiver(MessageHandlingService service)
//            {
//                _service = service;
//            }

//            public override void OnReceive(Context context, Intent intent)
//            {
//                _service.DisplayMessage(context, intent);
//            }
//        }
//    }
//}