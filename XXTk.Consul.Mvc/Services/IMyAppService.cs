using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.Consul.Mvc.Services
{
    public interface IMyAppService
    {
        Task<string> GetHelloAysnc();

        Task<List<string>> GetNamesAsync();
    }
}
