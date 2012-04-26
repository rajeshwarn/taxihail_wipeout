using System;

namespace apcurium.Framework.Extensions
{
    public interface IExtensionPoint
    {
        object ExtendedValue { get; }
        Type ExtendedType { get; }
    }
}