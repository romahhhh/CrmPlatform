using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public record ClientCreatedEvent(
        Guid ClientId,
        string Name,
        string Email
    );
}
