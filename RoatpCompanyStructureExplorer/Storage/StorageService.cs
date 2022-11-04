using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using RoatpCompanyStructureExplorer.Config;

namespace RoatpCompanyStructureExplorer.Storage
{
    public sealed class StorageService : IDisposable
    {
        private readonly string _tableName;
        private readonly SqlConnection _sqlConnection;

        public StorageService()
        {
            var configService = new ConfigurationService();
            var config = configService.GetAppConfig();

            _sqlConnection = new SqlConnection(config.DatabaseConnectionString);
            _tableName = "ProviderCompany";
        }

        public async Task StoreItem(CompanyRecord eventData)
        {
            var sql =
                $"insert into [{_tableName}] (CompanyNumber, RootCompanyNumber, ParentCompanyNumber, CompanyName, ProfileData, PscData, OfficersData, Ukprn, FilingHistoryData) values (@CompanyNumber, @RootCompanyNumber, @ParentCompanyNumber, @CompanyName, @ProfileData, @PscData, @OfficersData, @Ukprn, @FilingHistoryData)";

            try
            {
                await _sqlConnection.ExecuteAsync(sql, eventData);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error {ex.Message}");
            }
        }

        public async Task UpdateItem(CompanyRecord eventData)
        {
            var sql =
                $"update [{_tableName}] set FilingHistoryData = @FilingHistoryData where CompanyNumber = @CompanyNumber";
            //$"update [{_tableName}] set OfficersData = @OfficersData, ProfileData = @ProfileData, PscData = @PscData, FilingHistoryData = @FilingHistoryData where CompanyNumber = @CompanyNumber";

            try
            {
                await _sqlConnection.ExecuteAsync(sql, eventData);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error {ex.Message}");
            }
        }

        public void Dispose()
        {
            _sqlConnection?.Dispose();
        }
    }
}
