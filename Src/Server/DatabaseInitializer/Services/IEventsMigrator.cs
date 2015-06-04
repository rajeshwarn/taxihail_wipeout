using System;

namespace DatabaseInitializer.Services
{
    public interface IEventsMigrator
    {
        void Do(DateTime? after = null);
    }
}