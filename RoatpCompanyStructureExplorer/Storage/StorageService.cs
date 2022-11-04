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

        private async Task InsertItem(CompanyRecord eventData)
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
                //$"update [{_tableName}] set FilingHistoryData = @FilingHistoryData where CompanyNumber = @CompanyNumber";
            $"update [{_tableName}] set OfficersData = @OfficersData, ProfileData = @ProfileData, PscData = @PscData, FilingHistoryData = @FilingHistoryData where CompanyNumber = @CompanyNumber";

            try
            {
                await _sqlConnection.ExecuteAsync(sql, eventData);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error {ex.Message}");
            }
        }

        public async Task Store(CompanyRecord companyRecord)
        {
            if (await Exists(companyRecord))
            {
                await UpdateItem(companyRecord);
            }
            else
            {
                await InsertItem(companyRecord);
            }
        }

        private async Task<bool> Exists(CompanyRecord companyRecord)
        {
            var sql =
                $"select 1 from [{_tableName}] where CompanyNumber = @CompanyNumber";

            try
            {
                var result = await _sqlConnection.ExecuteScalarAsync<bool?>(sql, companyRecord);
                return result.HasValue && result.Value;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error {ex.Message}");
            }

            return false;
        }

        public void Dispose()
        {
            _sqlConnection?.Dispose();
        }
    }
}
