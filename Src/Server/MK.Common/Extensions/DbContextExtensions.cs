#region

using System;
using System.Data.Entity;
using System.Linq;

#endregion

namespace apcurium.MK.Common.Extensions
{
    public static class DbContextExtensions
    {
        public static void RemoveAll<T>(this DbContext thisObj) where T : class
        {
            var table = thisObj.Set<T>();

            if (!table.Any())
            {
                return;
            }

            var lines = thisObj.Set<T>();

            foreach (var entity in lines)
            {
                table.Remove(entity);
            }
        }

        public static void RemoveWhere<T>(this DbContext thisObj, Func<T, bool> clause) where T : class
        {
            var table = thisObj.Set<T>();

            if (!table.Any())
            {
                return;
            }

            var lines = thisObj.Set<T>().Where(clause);

            foreach (var entity in lines)
            {
                table.Remove(entity);
            }
        }
    }
}