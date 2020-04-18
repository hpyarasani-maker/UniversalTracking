using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalSendingKeywords
{
    class OxyParams
    {
        
        public string source { get; set; }
        public string domain { get; set; }
        public string[] query { get; set; }
        public int limit { get; set; }
        public int pages { get; set; }
        public int start_page { get; set; }
        public string locale { get; set; }
        public string geo_location { get; set; }
        public bool parse { get; set; }
        public string user_agent_type { get; set; }
        public string[] url { get; set; }
        public string callback_url { get; set; }
        public List<Context> context { get; set; }

    }
    class Context
    {
        public string key { get; set; }
        public dynamic value { get; set; }

        public Context(string key, dynamic value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
