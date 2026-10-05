using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Settings
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string Host { get; set; } = "localhost";
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public ushort Port { get; set; } = 5672;
    }
}
