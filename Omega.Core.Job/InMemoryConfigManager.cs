using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Job
{
    public class InMemoryConfigManager : IConfigManager
    {
        private static readonly IDictionary<string, string> Values = new Dictionary<string, string>
        {
            {"type", "InMemmory"}
        };

        public string GetValue(string key)
        {
            string value;

            if (Values.TryGetValue(key, out value))
            {
                return value;
            }

            return "Key not found!";
        }
    }
}
