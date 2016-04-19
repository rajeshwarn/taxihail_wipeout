﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Http
{
    public class SessionEntity
    {
        public string SessionId { get; set; }
        public string UserName { get; set; }
        public Guid UserId { get; set; }

        public bool IsAuthenticated()
        {
            return Guid.Empty != UserId;
        }
    }
}