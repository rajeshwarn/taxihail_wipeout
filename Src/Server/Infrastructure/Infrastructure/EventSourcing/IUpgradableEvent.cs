using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.EventSourcing
{
    public interface IUpgradableEvent
    {
        IVersionedEvent Upgrade();
    }
}
