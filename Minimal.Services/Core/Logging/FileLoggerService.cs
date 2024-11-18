using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Services.Core.Logging
{
    public class FileLoggerService : ILoggerService
    {
        private readonly ILogger<FileLoggerService> _logger;
        public FileLoggerService(ILogger<FileLoggerService> logger)
        {
            _logger = logger;
        }

        public async Task Log(Exception e)
        {
            _logger.LogError(e, e.Message);

            await Task.CompletedTask;
        }

        public async Task Log(string info)
        {
            _logger.LogInformation(info);

            await Task.CompletedTask;
        }
    }
}
