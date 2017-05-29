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
    public interface IProjectRepository
    {
        DataTable GetProject();
        DataTable GetProjectSummary(string projectIds, DateTime projectFromDate, DateTime projectToDate);
        DataTable GetPRBreakdown(int projectId);
        DataTable GetPOBreakdown(int projectId);
        DataTable GetMRRBreakdown(int projectId);
        DataTable GetBillBreakdown(int projectId);
        DataTable GetOtherBreakdown(int projectId);
    }
}
