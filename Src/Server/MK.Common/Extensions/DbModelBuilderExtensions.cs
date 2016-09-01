#region

using System.Data.Entity;

#endregion

namespace apcurium.MK.Common.Extensions
{
    public static class DbModelBuilderExtensions
    {
        public static void CreateTable<T>(this DbModelBuilder modelBuilder, string schemaName) where T : class
        {
            modelBuilder.Entity<T>().ToTable(typeof (T).Name, schemaName);
        }
    }
}