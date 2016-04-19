using System;

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
