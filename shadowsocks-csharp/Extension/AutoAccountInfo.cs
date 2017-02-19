using System.Collections.Generic;
using Shadowsocks.Model;

namespace Shadowsocks.Extension
{
    public class AutoAccountInfo
    {
        public string AllHtml { get; set; }

        public string AccountArea { get; set; }

        public List<AutoServer> AutoServers { get; set; }
    }
}