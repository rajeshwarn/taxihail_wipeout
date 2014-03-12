using System;
using System.Linq;
using System.Threading;
using Cirrious.CrossCore.Droid.Platform;
using Java.IO;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class PlatformHelper
    {
        public static int APILevel
        {
            get
            {
                return (int)Android.OS.Build.VERSION.SdkInt;
            }
        }

        public static bool IsAndroid23
        {
            get
            {
                return APILevel <= 10;
            }
        }

		static System.Timers.Timer timer = new System.Timers.Timer ();

		public static void BenchMarkStart()
		{
			timer = new System.Timers.Timer ();
			timer.Interval = 250;
			timer.AutoReset = true;
			timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => {
				readUsage();
			};
			timer.Start ();
		}

		public static void BenchMarkStop()
		{
			if (timer != null) {
				timer.Stop ();
				timer.Close ();
				timer.Dispose ();
			}
		}

		private static float[] cpuStack = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };

		private static int cpuStackPointer = 0;

		private static void readUsage() {
			try {
				var topActivity = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity> (); 
				RandomAccessFile reader = new RandomAccessFile ("/proc/stat", "r");
				string load = reader.ReadLine ();

				string[] toks = load.Split (' ');

				long idle1 = long.Parse (toks [5]);
				long cpu1 = long.Parse (toks [2]) + long.Parse (toks [3]) + long.Parse (toks [4])
					+ long.Parse (toks [6]) + long.Parse (toks [7]) + long.Parse (toks [8]);

				try {
					Thread.Sleep (360);
				} catch (Exception) {
				}

				reader.Seek (0);
				load = reader.ReadLine ();
				reader.Close ();

				toks = load.Split (' ');

				float idle2 = float.Parse (toks [5]);
				float cpu2 = float.Parse (toks [2]) + float.Parse (toks [3]) + float.Parse (toks [4])
					+ float.Parse (toks [6]) + float.Parse (toks [7]) + float.Parse (toks [8]);

				float cpuValue = ((cpu2 - cpu1) * 100f / ((cpu2 + idle2) - (cpu1 + idle1)));
				cpuStack [cpuStackPointer++] = cpuValue;

				if (cpuStackPointer == 10) {
					cpuStackPointer = 0;
				}
				var averageTxt = ((int)cpuStack.Take(10).Average()).ToString ().PadLeft(2,'0');

				TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));

				// Get VM Heap Size by calling:
				var heapSize = (Java.Lang.Runtime.GetRuntime().TotalMemory() / 1000).ToString().PadLeft(6,'0');

				// Get Allocated VM Memory by calling:
				var allocated = ((Java.Lang.Runtime.GetRuntime().TotalMemory() - Java.Lang.Runtime.GetRuntime().FreeMemory()) / 1000).ToString().PadLeft(6,'0');

				// Get Free Memory
				var free = (Java.Lang.Runtime.GetRuntime().FreeMemory() / 1000).ToString().PadLeft(6,'0');

				// Get VM Heap Size Limit by calling:
				var heapSizeLimit = (Java.Lang.Runtime.GetRuntime().MaxMemory() / 1000).ToString().PadLeft(6,'0');

				var mem = heapSize + " " + allocated + " " + free + " " + heapSizeLimit;

				System.Console.WriteLine ("-|-cpu " + (((long)t.TotalSeconds * 1000L) + (long)DateTime.Now.Millisecond).ToString() + " " + averageTxt + " " + mem + " " + topActivity.Activity.Title);


			} catch (System.IO.IOException) {

			}
		}
    }
}

