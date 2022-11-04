namespace RoatpCompanyStructureExplorer
{
    public class QueueItem
    {
        public string CompanyNumber { get; }
        public string Ukprn { get; set; }
        public string RootCompanyNumber { get; }
        public string ParentCompanyNumber { get; }
        public string CompanyName { get; }

        public QueueItem(string companyNumber, string ukprn, string rootCompanyNumber, string parentCompanyNumber, string companyName)
        {
            CompanyNumber = companyNumber;
            Ukprn = ukprn;
            RootCompanyNumber = rootCompanyNumber;
            ParentCompanyNumber = parentCompanyNumber;
            CompanyName = companyName;
        }
    }
}
