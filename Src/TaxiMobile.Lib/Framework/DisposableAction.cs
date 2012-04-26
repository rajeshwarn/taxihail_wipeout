﻿using System;

namespace TaxiMobile.Lib.Framework
{
    public class DisposableAction : IDisposable
    {
        public DisposableAction(Action action)
        {
            Action = action;
        }

        public Action Action { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            Action();
        }

        #endregion
    }
}
