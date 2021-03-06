﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                    Task.Delay(retryInterval).Wait();
                }
            }

            throw new AggregateException(exceptions);
        }


        public static void Retry(this Func<bool> func, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (var retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    var isSuccessful = func();
                    if (isSuccessful)
                    {
                        return;
                    }                    
                    else
                    {
                        exceptions.Add(new Exception("Method returned false"));
                        Task.Delay(retryInterval).Wait();
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Task.Delay(retryInterval).Wait();
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}