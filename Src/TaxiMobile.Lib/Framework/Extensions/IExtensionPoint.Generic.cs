﻿namespace TaxiMobile.Lib.Framework.Extensions
{
    public interface IExtensionPoint<T> : IExtensionPoint
    {
        new T ExtendedValue { get; }
    }
}