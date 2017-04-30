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
    public interface ILetterCreditRepository
    {
        DataTable GetStatus();
        DataTable GetBank();
        DataTable GetTerm();
        DataTable GetSupplier();
        DataTable GetLCSummary(string statusIds, string supplierIds, string bankIds, string termIds, DateTime issueFromDate, DateTime issueToDate);
        DataTable GetLCItems(int id);
        DataTable GetLCExpenditures(int id);
        DataTable GetLCActivities(int id);
    }
}
