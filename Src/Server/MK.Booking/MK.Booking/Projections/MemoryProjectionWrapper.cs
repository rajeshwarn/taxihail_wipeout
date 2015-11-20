using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Projections
{
    public class ProjectionWrapper<TProjection> : IProjection<TProjection> where TProjection: class
    {
        private Func<TProjection> _load;
        private Action<TProjection> _save;
        public ProjectionWrapper(Func<TProjection> load, Action<TProjection> save)
        {
            _load = load;
            _save = save;
        }

        public TProjection Load()
        {
            return _load();
        }

        public void Save(TProjection projection)
        {
            _save(projection);
        }
    }
}
