using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Services.Core.Logging
{
    public interface ILoggerService
    {
        Task Log(Exception e);

        Task Log(string info);
    }
}
