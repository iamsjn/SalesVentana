using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesVentana.BO;
using System.Linq.Expressions;
using System.Data;

namespace SalesVentana.Data
{
    public interface IReceivableSalesRepository
    {
        DataTable GetWorkorder();
        DataTable GetCustomer();
        DataTable GetChannel();
        DataTable GetSalesPerson();
        DataTable GetRSSummary(string channelIds, string workorderIds, string customerIds, string salesPersonIds);
        DataTable GetRSDetail(int id);
    }
}
