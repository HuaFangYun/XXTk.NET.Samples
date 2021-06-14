using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.Consul.EmailSender
{
    public class ConsulMessage
    {
        public string Node { get; set; }

        public string ServiceID { get; set; }

        public string ServiceName { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public string Output { get; set; }
    }
}
