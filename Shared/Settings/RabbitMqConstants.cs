using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Settings
{
    public static class RabbitMqConstants
    {
        public const string Exchange = "crm.events";

        public const string UserRegisteredRoutingKey = "user.registered";
        public const string ClientCreatedRoutingKey = "client.created";
        public const string SessionPlannedRoutingKey = "session.planned";
    }
}
