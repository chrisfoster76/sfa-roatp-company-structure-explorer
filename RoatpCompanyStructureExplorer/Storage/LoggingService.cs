using System.Collections.Generic;
using RoatpCompanyStructureExplorer.Config;
using System.IO;
using System.Threading.Tasks;

namespace RoatpCompanyStructureExplorer.Storage
{
    internal class LoggingService
    {
        private readonly string _fileName;

        public LoggingService()
        {
            var configService = new ConfigurationService();
            var config = configService.GetAppConfig();

            _fileName = config.LogFile;
        }

        public async Task Log(string message)
        {
            await File.AppendAllLinesAsync(_fileName, new List<string> { message });
        }
    }
}
