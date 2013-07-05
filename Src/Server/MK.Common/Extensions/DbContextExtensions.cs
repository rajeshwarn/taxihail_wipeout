using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
