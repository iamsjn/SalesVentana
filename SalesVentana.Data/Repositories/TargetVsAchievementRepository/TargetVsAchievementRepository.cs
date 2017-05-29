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
    public class TargetVsAchievementRepository : ITargetVsAchievementRepository
    {
        private SalesVentanaConnection _dbConnection;
        private string _sqlQuery;
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
        public TargetVsAchievementRepository(IDbFactory dbFactory)
        {
            DbFactory = dbFactory;
        }
        #endregion

        public DataSet GetPurchaseOrderDetail()
        {
            DataSet ds = new DataSet();
            _sqlQuery = string.Format(@"SELECT bp.BPID, bp.Name Supplier,count(po.ProjectID) POQty,sum(POAmount) POAmount,sum(mrri.MRRNetAmount) MRRAmount,
                                        sum(bap.NetAmount) Advance,sum(pmt.Amount) BillPaid,(sum(mrri.MRRNetAmount)-sum(bap.NetAmount))+sum(pmt.Amount) Payable
                                        FROM PurchaseOrder po,MaterialReceiptReport mrr,MaterialReceiptReportitem mrri, Businesspartner bp,
                                        BillProcessItem bpi,BillAdvanceProcess bap,PaymentDetails pd,Payments pmt
                                        WHERE po.ProjectID IS NOT NULL
                                        AND po.SupplierID=bp.BPID
                                        AND po.POID=mrr.POID
                                        AND mrr.MRRID=mrri.MRRID
                                        AND mrri.MRRID=bpi.MRRID
                                        AND bpi.BillProcessID=bap.BillAdvanceProcessID
                                        AND bap.BillAdvanceProcessID=pd.BillAdvanceProcessID
                                        AND pd.PaymentID=pmt.PaymentID
                                        GROUP BY bp.BPID, bp.Name;");
            ds.Tables.Add(ExecuteDataTable());

            _sqlQuery = string.Format(@"SELECT * FROM (
                                        SELECT sku.SKUID, sku.Name ,
                                        count(po.ProjectID) POQty,
                                        isnull(sum(POAmount),0) POAmount,
                                        isnull(sum(mrri.MRRNetAmount),0) MRRAmount
                                        FROM PurchaseOrder po
                                        LEFT OUTER JOIN 
                                        MaterialReceiptReport mrr ON po.POID=mrr.POID
                                        LEFT OUTER JOIN 
                                        MaterialReceiptReportitem mrri ON mrr.MRRID=mrri.MRRID
                                        LEFT OUTER JOIN 
                                        SKUs sku ON mrri.SKUID= sku.SKUID
                                        LEFT OUTER JOIN 
                                        BillProcessItem bpi ON mrri.MRRID=bpi.MRRID
                                        LEFT OUTER JOIN 
                                        BillAdvanceProcess bap ON bpi.BillProcessID=bap.BillAdvanceProcessID
                                        LEFT OUTER JOIN 
                                        PaymentDetails pd ON bap.BillAdvanceProcessID=pd.BillAdvanceProcessID
                                        LEFT OUTER JOIN 
                                        Payments pmt ON pd.PaymentID=pmt.PaymentID
                                        WHERE po.ProjectID IS NOT NULL
                                        GROUP BY sku.SKUID,sku.Name) tab1
                                        WHERE skuid IS NOT null;");
            ds.Tables.Add(ExecuteDataTable());

            return ds;
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
