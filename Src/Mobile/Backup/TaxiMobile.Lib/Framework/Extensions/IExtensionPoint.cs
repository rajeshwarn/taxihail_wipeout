using System;

namespace TaxiMobile.Lib.Framework.Extensions
{
    public interface IExtensionPoint
    {
        object ExtendedValue { get; }
        Type ExtendedType { get; }
    }
}