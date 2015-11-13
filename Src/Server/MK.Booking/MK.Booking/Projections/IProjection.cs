using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Projections
{
    public interface IProjection<TProjection> where TProjection : class
    {
        TProjection Load();
        void Save(TProjection projection);
    }
}
