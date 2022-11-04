using System.Collections.Generic;
using System.Linq;
using RoatpCompanyStructureExplorer.Models;

namespace RoatpCompanyStructureExplorer
{
    public class ProcessingQueue
    {
        private readonly List<QueueItem> _toDo;
        private readonly List<QueueItem> _done;

        public ProcessingQueue(IEnumerable<RoatpProvider> companyNumbers)
        {
            _done = new List<QueueItem>();
            _toDo = new List<QueueItem>();

            foreach (var c in companyNumbers)
            {
                _toDo.Add(new QueueItem(c.CompanyNumber, c.Ukprn, null, null, c.Name));
            }
        }

        public QueueItem GetNext()
        {
            var next = _toDo.FirstOrDefault();

            if (next == null) return null;

            _done.Add(next);
            _toDo.Remove(next);

            return next;
        }

        public void Add(QueueItem newItem)
        {
            _toDo.Insert(0, newItem);
            //_toDo.Add(newItem);
        }

        public bool IsEmpty
        {
            get
            {
                return _toDo.Count == 0;
            }
        }

        public IReadOnlyCollection<QueueItem> GetDoneItems()
        {
            return _done.AsReadOnly();
        }

        public bool HasProcessed(string companyNumber, string rootCompanyNumber)
        {
            if (_done.Any(x => x.CompanyNumber == companyNumber && x.RootCompanyNumber == rootCompanyNumber))
            {
                return true;
            }

            if (_toDo.Any(x => x.CompanyNumber == companyNumber && x.RootCompanyNumber == rootCompanyNumber))
            {
                return true;
            }

            return false;
        }
    }
}
