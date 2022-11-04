using System.Dynamic;

namespace RoatpCompanyStructureExplorer.Storage
{
    public class CompanyRecord
    {
        public string CompanyNumber { get; set; }
        public string RootCompanyNumber { get; set; }
        public string ParentCompanyNumber { get; set; }
        public string CompanyName { get; set; }
        public string ProfileData { get; set; }
        public string PscData { get; set; }
        public string OfficersData { get; set; }
        public string Ukprn { get; set; }
        public string FilingHistoryData { get; set; }
    }
}
