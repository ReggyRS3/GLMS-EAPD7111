using GLMS.Web.Models;

namespace GLMS.Web.Services
{
    public static class ContractFilter
    {
        public static IQueryable<Contract> Apply(
            IQueryable<Contract> query,
            DateTime? fromDate,
            DateTime? toDate,
            ContractStatus? status)
        {
            if (status.HasValue)
            {
                var selectedStatus = status.Value;
                query = query.Where(c => c.Status == selectedStatus);
            }

            // Contract must still be running on or after the "from" date
            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                query = query.Where(c => c.EndDate >= from);
            }

            // Contract must have started on or before the "to" date
            if (toDate.HasValue)
            {
                var to = toDate.Value.Date;
                query = query.Where(c => c.StartDate <= to);
            }

            return query;
        }
    }
}