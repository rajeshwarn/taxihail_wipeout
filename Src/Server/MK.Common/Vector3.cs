using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Common
{
	public struct Vector3
	{
		public double x, y, z;

		public Vector3(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public void Normalize()
		{
			double vectorLength = Math.Pow(x * x + y * y + z * z, 0.5);

			x /= vectorLength;
			y /= vectorLength;
			z /= vectorLength;
		}
	}
}