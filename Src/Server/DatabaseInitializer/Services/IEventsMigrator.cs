﻿namespace DatabaseInitializer.Services
{
    public interface IEventsMigrator
    {
        void Do(string version);
    }
}