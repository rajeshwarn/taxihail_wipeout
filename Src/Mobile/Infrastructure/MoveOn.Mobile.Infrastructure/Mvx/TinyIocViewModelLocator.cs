
#region Copyright
// <copyright file="MvxDefaultViewModelLocator.cs" company="Cirrious">
// (c) Copyright Cirrious. http://www.cirrious.com
// This source is subject to the Microsoft Public License (Ms-PL)
// Please see license.txt on http://opensource.org/licenses/ms-pl.html
// All other rights reserved.
// </copyright>
// 
// Project Lead - Stuart Lodge, Cirrious. http://www.cirrious.com
using Cirrious.MvvmCross.IoC;
using TinyIoC;

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Platform.Diagnostics;

namespace MK.Booking.Mobile.Infrastructure.Mvx
{
    public class TinyIocViewModelLocator : IMvxViewModelLocator
    {
        #region IMvxViewModelLocator Members

        public bool TryLoad(Type viewModelType, IDictionary<string, string> parameters, out IMvxViewModel model)
        {
            model = null;

            var dict = new Dictionary<string,object>();

            if ((parameters != null) && (parameters.Count > 0))
            {
                foreach (var item in parameters)
                {
                    dict.Add(item.Key, item.Value);
                }

                model = TinyIoCContainer.Current.Resolve(viewModelType, new NamedParameterOverloads(dict), ResolveOptions.Default)as IMvxViewModel;           
            }
            else
            {
                model = TinyIoCContainer.Current.Resolve(viewModelType)as IMvxViewModel;           
            }

            return (model != null);
        }

        #endregion
    }
}
