﻿#region

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace apcurium.MK.Common.Extensions
{
    public static class ActionExtension
    {
        public static void Retry(this Action action, TimeSpan retryInterval, int retryCount = 3)
        {
            Retry<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount);
        }

        public static T Retry<T>(this Func<T> action, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (var retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}