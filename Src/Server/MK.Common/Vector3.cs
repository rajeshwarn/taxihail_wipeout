using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
