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


#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Platform.Diagnostics;

namespace Cirrious.MvvmCross.Application
{
    public class MvxDefaultViewModelLocator
        : IMvxViewModelLocator
    {
        #region IMvxViewModelLocator Members

        public bool TryLoad(Type viewModelType, IDictionary<string, string> parameters, out IMvxViewModel model)
        {
            model = null;
            var loadMethod = viewModelType
#if NETFX_CORE
                .GetTypeInfo().DeclaredConstructors
#else
                .GetMethods( )
#endif
                .FirstOrDefault(c => c.Name == "Load" &&  c.GetParameters().All(p=> p.ParameterType == typeof(string)));

            if (loadMethod == null)
            {
                MvxTrace.Trace("Missing Load method to {0} ", viewModelType );
                return false;
            }
            var invokeWith = new List<object>();
            foreach (var parameter in loadMethod.GetParameters())
            {
                string parameterValue = null;
                if (parameters == null ||
                    !parameters.TryGetValue(parameter.Name, out parameterValue))
                {
                    MvxTrace.Trace("Missing parameter in call to {0} - missing parameter {1} - asssuming null", viewModelType,
                                   parameter.Name);
                }
                invokeWith.Add(parameterValue);
            }


            model = MvxOpenNetCfContainer.Current.Resolve( viewModelType )as IMvxViewModel;

            loadMethod.Invoke( model, invokeWith.ToArray() );

            //model = Activator.CreateInstance(viewModelType, invokeWith.ToArray()) as IMvxViewModel;
            return (model != null);
        }

        #endregion
    }
}