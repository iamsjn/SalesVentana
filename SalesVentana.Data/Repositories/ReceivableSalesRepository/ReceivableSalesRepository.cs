using Autofac;
using SalesVentana.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.Data
{
    public class ReceivableSalesRepository : IReceivableSalesRepository
    {
        private SalesVentanaConnection _dbConnection;
        private string _sqlQuery;
        string _employeeTable;
        string _employeeSearch;
        private SqlCommand _command;

        #region Properties
        protected IDbFactory DbFactory
        {
            get;
            private set;
        }
        protected SalesVentanaConnection DbConnection
        {
            get { return _dbConnection ?? (_dbConnection = DbFactory.Initialize()); }
        }

        public ReceivableSalesRepository(IDbFactory dbFactory)
        {
            DbFactory = dbFactory;
        }
        #endregion
        public DataTable GetChannel()
        {
            _sqlQuery = string.Format(@"Select Id as ChannelId, Name as ChannelName From vw_DimChannel;");
            return ExecuteDataTable();
        }

        public DataTable GetWorkorder()
        {
            _sqlQuery = string.Format(@"Select SalesWorkOrderId WorkorderId, WorkOrderNo WorkorderName From vw_workorderfact;");
            return ExecuteDataTable();
        }

        public DataTable GetCustomer()
        {
            _sqlQuery = string.Format(@"Select BPId CustomerId, Name CustomerName From vw_DimCustomer;");
            return ExecuteDataTable();
        }

        public DataTable GetSalesPerson()
        {
            _sqlQuery = string.Format(@"Select EmployeeId SalesPersonId, Name SalesPersonName From Employees;");
            return ExecuteDataTable();
        }

        public DataTable GetTerm()
        {
            _sqlQuery = string.Format(@"Select PaymentOptionId TermId, Name TermName From vw_DimLCTermsofPaymentOption;");
            return ExecuteDataTable();
        }

        public DataTable GetRSSummary(string channelIds, string workorderIds, string customerIds, string salesPersonIds)
        {
            string query = string.Empty;
            string stringToConcat = string.Empty;

            if (!string.IsNullOrEmpty(channelIds))
                query = GetQuery(query, " wo.ChannelType IN(" + channelIds.Trim(',') + ")");
            if (!string.IsNullOrEmpty(workorderIds))
                query = GetQuery(query, " wo.SalesWorkOrderId IN(" + workorderIds.Trim(',') + ")");
            if (!string.IsNullOrEmpty(customerIds))
                query = GetQuery(query, " wo.CustomerId IN(" + customerIds.Trim(',') + ")");
            if (!string.IsNullOrEmpty(salesPersonIds))
                query = GetQuery(query, " wo.EmployeeID IN(" + salesPersonIds.Trim(',') + ")");

            _sqlQuery = string.Format(@"SELECT bp.BPID customerID, bp.Name 'Customer Name',count(wo.WorkOrderNo) 'No. Of W/O' ,isnull(sum(wo.TotalAmount),0) 'Work Order Value',0 'Bill Amount(Tk.)',
                                        isnull(advance.amount,0) 'Advance Receive(Tk.)',isnull(receive.amount,0) 'Receive Amount(Tk.)',
                                        isnull(advance.amount,0)+isnull(receive.amount,0) 'Total Receive Amt(Tk.)',
                                        isnull(sum(wo.TotalAmount),0)-(isnull(advance.amount,0)+isnull(receive.amount,0)) 'Receivable Amount(Tk.)'
                                        FROM vw_WorkOrderFact wo
                                        LEFT OUTER JOIN BusinessPartner bp ON wo.CustomerID=bp.BPID
                                        LEFT OUTER JOIN vw_DimAdvance advance ON wo.customerid=advance.bpid
                                        LEFT OUTER JOIN vw_DimReceived receive ON wo.customerid=receive.bpid
                                        {0}
                                        GROUP BY bp.BPID,bp.Name,advance.amount,receive.amount;", query);
            return ExecuteDataTable();
        }

        public DataTable GetRSDetail(int id)
        {
            _sqlQuery = string.Format(@"SELECT  convert(NVARCHAR, a1, 106) 'W/O Date',a2 'Work Order Number',a3 'W/O Value',a4 'Total Bill Amount',a5 'No. of Bills',
                                        a6 'Advance Received (Tk.)',a7 'Receive Amount(Tk.)',a8 'Total Received (Tk.)',a9 'Receivable Amount(Tk.)' FROM(
                                        SELECT  a.WorkOrderDate a1,a.WorkOrderNo a2,a.TotalAmount a3 ,0 a4,count(noofbills)  a5,
                                        isnull(sum(advance.amount),0) a6,isnull(sum(receive.amount),0) a7,isnull(sum(advance.amount),0)+isnull(sum(receive.amount),0) a8,
                                        isnull(sum(a.TotalAmount),0)-(isnull(sum(advance.amount),0)+isnull(sum(receive.amount),0)) a9
                                        FROM vw_WorkOrderFact a
                                        LEFT OUTER JOIN (SELECT WorkOrderID,amount,memoID FROM vw_SalesMemoFact b
                                        WHERE b.isadvance=1) advance ON a.SalesWorkOrderID=advance.WorkOrderID
                                        LEFT OUTER JOIN (SELECT WorkOrderID,amount,memoID FROM vw_SalesMemoFact b
                                        WHERE b.isadvance=0) receive ON a.SalesWorkOrderID=receive.WorkOrderID
                                        LEFT OUTER JOIN vw_DimBillCount bill ON a.SalesWorkOrderID=bill.WorkOrderID
                                        WHERE a.CustomerID= {0}
                                        GROUP BY a.WorkOrderDate ,a.WorkOrderNo ,a.TotalAmount
                                        )tab1
                                        GROUP BY a1,a2,a3,a4,a5,a6,a7,a8,a9;", id);
            return ExecuteDataTable();
        }

        private string GetQuery(string currentQuery, string stringToConcat)
        {
            if (currentQuery.Contains("Where"))
                currentQuery += "AND" + stringToConcat;
            else
                currentQuery += "Where" + stringToConcat;
            return currentQuery;
        }

        private IDataReader ExecuteReader()
        {
            _command = new SqlCommand(_sqlQuery, DbConnection.SqlConnection);
            return _command.ExecuteReader();
        }

        private DataTable ExecuteDataTable()
        {
            DataTable table = new DataTable();
            _command = new SqlCommand(_sqlQuery, DbConnection.SqlConnection);
            table.Load(_command.ExecuteReader());
            return table;
        }
    }
}
