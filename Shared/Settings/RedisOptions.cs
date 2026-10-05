using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Settings
{
    public class RedisOptions
    {
        public const string SectionName = "Redis";

        public string ConnectionString { get; set; } = "localhost:6379";
        public int SessionsTtlMinutes { get; set; } = 5;
    }
}
