using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.Consul.EmailSender
{
    public interface INoticeEmailSender
    {
        Task SendAsync(IEnumerable<ConsulMessage> messages);
    }
}
