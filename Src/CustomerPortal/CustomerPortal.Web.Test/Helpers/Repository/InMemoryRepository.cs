#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Test.Helpers.Repository
{
    public class InMemoryRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly List<T> _db = new List<T>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _db.GetEnumerator();
        }

        public Expression Expression
        {
            get { return ((IQueryable) _db).Expression; }
        }

        public Type ElementType
        {
            get { return ((IQueryable) _db).ElementType; }
        }

        public IQueryProvider Provider
        {
            get { return ((IQueryable) _db).Provider; }
        }

        public T GetById(string id)
        {
            return _db.SingleOrDefault(x => x.Id == id);
        }

        public T GetSingle(Expression<Func<T, bool>> criteria)
        {
            return _db.Single(criteria.Compile());
        }

        public T Add(T entity)
        {
            if (entity.Id == null) throw new Exception("Id cannot be null");

            _db.Add(entity);
            return entity;
        }

        public void Add(IEnumerable<T> entities)
        {
            _db.AddRange(entities);
        }

        public T Update(T entity)
        {
            var toBeReplaced = _db.Single(x => x.Id == entity.Id);
            _db.Remove(toBeReplaced);
            _db.Add(entity);
            return entity;
        }

        public void Update(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }

        public void Delete(string id)
        {
            var toBeRemoved = _db.Single(x => x.Id == id);
            _db.Remove(toBeRemoved);
        }

        public void Delete(T entity)
        {
            var toBeRemoved = _db.Single(x => x.Id == entity.Id);
            _db.Remove(toBeRemoved);
        }

        public void Delete(Expression<Func<T, bool>> criteria)
        {
            var compiled = criteria.Compile();
            _db.RemoveAll(compiled.Invoke);
        }

        public void DeleteAll()
        {
            _db.Clear();
        }

        public long Count()
        {
            return _db.LongCount();
        }

        public bool Exists(Expression<Func<T, bool>> criteria)
        {
            var compiled = criteria.Compile();
            return _db.Any(compiled.Invoke);
        }

        public IDisposable RequestStart()
        {
            return new NullDisposable();
        }

        public void RequestDone()
        {
        }

        public MongoCollection<T> Collection { get; private set; }

        private class NullDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}