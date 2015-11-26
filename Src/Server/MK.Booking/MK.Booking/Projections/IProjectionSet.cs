using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Projections
{
    public interface IProjectionSet<TProjection> : IProjectionSet<TProjection, Guid> where TProjection : class
    {
    }

    public interface IProjectionSet<TProjection, TIdentifier> where TProjection : class
    {
        void Update(TIdentifier identifier, Action<TProjection> action);
        void Update(Func<TProjection, bool> predicate, Action<TProjection> action);

        void Add(TProjection projection);
        void AddOrReplace(TProjection projection);
        void AddRange(IEnumerable<TProjection> projections);

        bool Exists(TIdentifier identifier);
        bool Exists(Func<TProjection, bool> predicate);

        void Remove(TIdentifier identifier);
        void Remove(Func<TProjection, bool> predicate);

        IProjection<TProjection> GetProjection(TIdentifier identifier);
    }

    public interface IAppendOnlyProjectionSet<TProjection> where TProjection : class
    {
        void Add(TProjection projection);
        void AddRange(IEnumerable<TProjection> projections);
    }
}
