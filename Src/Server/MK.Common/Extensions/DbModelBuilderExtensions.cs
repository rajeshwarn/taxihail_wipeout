using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Extensions
{
    public static class DbModelBuilderExtensions
    {
        public static void CreateTable<T>(this DbModelBuilder modelBuilder, string schemaName) where T : class
        {
            modelBuilder.Entity<T>().ToTable(typeof(T).Name, schemaName);
        }
    }
}
