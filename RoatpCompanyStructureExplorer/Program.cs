using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JackLeitch.RateGate;
using Polly;
using RoatpCompanyStructureExplorer.Config;
using RoatpCompanyStructureExplorer.Models;
using RoatpCompanyStructureExplorer.Policies;
using RoatpCompanyStructureExplorer.Storage;

namespace RoatpCompanyStructureExplorer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configService = new ConfigurationService();
            var config = configService.GetAppConfig();

            using var storageService = new StorageService();

            var providerService = new RoatpService();
            var providers = providerService.GetProviders();

            var queue = new ProcessingQueue(providers);

            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.company-information.service.gov.uk");

            var _apiUser = config.ApiUser;

            var authToken = Encoding.ASCII.GetBytes($"{_apiUser}:"); //username with no password
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

            using var rateGate = new RateGate(2, TimeSpan.FromSeconds(10));

            var count = 0;

            Console.Write(".");

            while (!queue.IsEmpty)
            {
                rateGate.WaitToProceed();

                count++;

                Console.CursorLeft = 0;
                Console.Write($"{count}");

                var company = queue.GetNext();

                var profileUri = $"/company/{company.CompanyNumber}";
                var pscUri = $"/company/{company.CompanyNumber}/persons-with-significant-control?items_per_page=999";
                var officersUrl = $"/company/{company.CompanyNumber}/officers?items_per_page=999";
                var filingHistoryUrl = $"/company/{company.CompanyNumber}/filing-history?items_per_page=999";


                var profileResponse = await Policy
                    .WrapAsync(RetryPolicies.RetryPolicy, RetryPolicies.BackOffPolicy)
                    .ExecuteAsync(() => _httpClient.GetAsync(profileUri));

                //var x = await Policy
                //    .Handle<HttpRequestException>()
                //    .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                //        (ex, timeSpan, context) =>
                //        {
                //            //DO Something
                //        })
                //    .ExecuteAsync(() => _httpClient.GetAsync(profileUri));


                var pscResponse = await Policy
                    .WrapAsync(RetryPolicies.RetryPolicy, RetryPolicies.BackOffPolicy)
                    .ExecuteAsync(() => _httpClient.GetAsync(pscUri));


                var officersResponse = await Policy
                    .WrapAsync(RetryPolicies.RetryPolicy, RetryPolicies.BackOffPolicy)
                    .ExecuteAsync(() => _httpClient.GetAsync(officersUrl));


                var filingHistoryResponse = await Policy
                    .WrapAsync(RetryPolicies.RetryPolicy, RetryPolicies.BackOffPolicy)
                    .ExecuteAsync(() => _httpClient.GetAsync(filingHistoryUrl));
                

                var profileData = await profileResponse.Content.ReadAsStringAsync();
                var pscData = await pscResponse.Content.ReadAsStringAsync();
                var officersData = await officersResponse.Content.ReadAsStringAsync();
                var filingHistoryData = await filingHistoryResponse.Content.ReadAsStringAsync();

                var profile = JsonSerializer.Deserialize<CompanyProfileResponse>(profileData);
                var pscs = JsonSerializer.Deserialize<PersonsWithSignificantControlListResponse>(pscData);
                var officers = JsonSerializer.Deserialize<OfficersResponse>(officersData);

                if (profile.company_status != "active")
                {
                    continue;
                }

                var companyRecord = new CompanyRecord
                {
                    CompanyNumber = company.CompanyNumber,
                    ParentCompanyNumber = company.ParentCompanyNumber,
                    RootCompanyNumber = company.RootCompanyNumber,
                    CompanyName = profile.company_name,
                    ProfileData = profileData,
                    PscData = pscData,
                    OfficersData = officersData,
                    Ukprn = company.Ukprn,
                    FilingHistoryData = filingHistoryData
                };

                await storageService.Store(companyRecord);

                if (pscs.items != null)
                {
                    foreach (var item in pscs.items)
                    {
                        if (!item.ceased_on.HasValue && item.identification != null && item.identification.registration_number != company.CompanyNumber)
                        {
                            if (!string.IsNullOrWhiteSpace(item.identification.registration_number))
                            {
                                if (queue.HasProcessed(item.identification.registration_number,
                                        company.RootCompanyNumber ?? company.CompanyNumber))
                                {
                                    Console.WriteLine($"Duplicate {item.identification.registration_number} detected ");
                                    continue;
                                }

                                Console.Write($"Adding company {item.identification.registration_number} as parent (psc) of {company.CompanyNumber}");

                                queue.Add(new QueueItem(item.identification.registration_number, company.Ukprn,
                                    company.RootCompanyNumber ?? company.CompanyNumber, company.CompanyNumber,
                                    item.name));
                            }
                            else if (item.kind != "legal-person-person-with-significant-control")
                            {
                                Console.WriteLine($"Empty registration number: {company.CompanyNumber} : {item.kind}");
                            }
                        }
                    }
                }

                if (officers.items != null)
                {
                    foreach (var item in officers.items)
                    {
                        if (!item.resigned_on.HasValue && item.identification != null && item.identification.registration_number != company.CompanyNumber)
                        {
                            if (!string.IsNullOrWhiteSpace(item.identification.registration_number))
                            {
                                if (queue.HasProcessed(item.identification.registration_number,
                                        company.RootCompanyNumber ?? company.CompanyNumber))
                                {
                                    Console.WriteLine($"Duplicate {item.identification.registration_number} detected ");
                                    continue;
                                }

                                Console.Write($"Adding company {item.identification.registration_number} as parent (officer) of {company.CompanyNumber}");

                                queue.Add(new QueueItem(item.identification.registration_number, company.Ukprn,
                                    company.RootCompanyNumber ?? company.CompanyNumber, company.CompanyNumber,
                                    item.name));
                            }
                            //else if (item.kind != "legal-person-person-with-significant-control")
                            //{
                            //    Console.WriteLine($"Empty registration number: {company.CompanyNumber} : {item.kind}");
                            //}
                        }
                    }
                }

            }

            Console.WriteLine();
            Console.WriteLine("Output:");
            foreach (var d in queue.GetDoneItems())
            {
                Console.Write($"{d.CompanyNumber} - {d.CompanyName}");
                if (!string.IsNullOrWhiteSpace(d.RootCompanyNumber))
                {
                    Console.Write($" - root -> {d.RootCompanyNumber}");
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Complete");
            Console.ReadKey();
        }
    }
}
