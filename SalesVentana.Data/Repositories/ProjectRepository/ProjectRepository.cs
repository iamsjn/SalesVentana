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
    public class ProjectRepository : IProjectRepository
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

        public ProjectRepository(IDbFactory dbFactory)
        {
            DbFactory = dbFactory;
        }
        #endregion
        public DataTable GetProject()
        {
            _sqlQuery = string.Format(@"Select ProjectId, ProjectName From Projects;");
            return ExecuteDataTable();
        }

        public DataTable GetProjectSummary(string projectIds, DateTime projectFromDate, DateTime projectToDate)
        {
            string query = string.Empty;

            if (!string.IsNullOrEmpty(projectIds))
                query = GetQuery(query, " prj.ProjectId In(" + projectIds.Trim(',') + ") ");

            if (projectFromDate != null && projectFromDate != DateTime.MinValue)
                query = GetQuery(query, " CAST(prj.CreatedDate AS DATE)>='" + projectFromDate + "' ");

            if (projectToDate != null && projectToDate != DateTime.MinValue)
                query = GetQuery(query, " CAST(prj.CreatedDate AS DATE)<='" + projectToDate + "' ");

            _sqlQuery = string.Format(@"Select T.PRQuantity,T2.POAmount,T3.MRRAmount,T3.BillAmount,T4.TotalBudget,T5.OtherAmount, (IsNull(T4.TotalBudget, 0) - (IsNull(T2.POAmount, 0) + IsNull(T5.OtherAmount, 0))) As 'BudgetVariance',
                                         prj.ProjectID, prj.ProjectName, CONVERT(VARCHAR(11), prj.CreationDate ,106) As 'CreationDate', prj.Description
                                         from Projects prj
                                         LEFT JOIN
                                         (Select ISNULL(ProjectID,0) ProjectID,Count(*) PRQuantity from Requisitions r
                                          Group by ProjectID) T On prj.ProjectID=T.ProjectID
                                          LEFT JOIN
                                         (Select ISNULL(ProjectID,0) ProjectID,Sum(POAmount) POAmount from PurchaseOrder
                                          Group by ProjectID) T2  On prj.ProjectID=T2.ProjectID
                                          LEFT JOIN
                                         (Select ISNULL(po.ProjectID,0) ProjectID,Sum(MRRNetAmount) MRRAmount,Sum(ProcessAmount) BillAmount from BillProcessItem bpi
                                          INNER JOIN MaterialReceiptReport mrr on bpi.MRRID= mrr.MRRID
                                          INNER JOIN PurchaseOrder po ON mrr.POID=po.POID
                                          Group by po.ProjectID) T3 On prj.ProjectID=T3.ProjectID
                                          LEFT JOIN
                                         (Select ISNULL(pj.ProjectID,0) ProjectID,Sum(ISNULL(Amount,0)) TotalBudget from BOQHierarchy bq
                                          INNER JOIN Projects pj on pj.ProjectID=bq.ProjectID
                                          Group by pj.ProjectID) T4 On prj.ProjectID=T4.ProjectID 
                                          LEFT JOIN
										 (Select Isnull(ReferenceID,0) ProjectID,SUM(ISNULL(AmountBDT,0)) OtherAmount From FinancialRequesitionItem fri
										  INNER JOIN FinancialRequisition fr ON fr.FinancialRequisitionID=fri.FinancialRequisitionID
									     Where ReferenceType=4 Group by ReferenceID) T5 On prj.ProjectID=T5.ProjectID {0}", query);
            return ExecuteDataTable();
        }

        public DataTable GetPRBreakdown(int projectId)
        {
            _sqlQuery = string.Format(@"Select T.*, T2.POAmount, T3.MRRAmount, T3.BillAmount FROM
                                        (Select ISNULL(r.ProjectID, 0) ProjectID, ri.SKUID, sk.Name SKUName,
                                        SUM(ISNULL(ri.ApprovedQty, 0)) PRQuantity from RequisitionItems ri
                                        INNER JOIN SKUs sk ON sk.SKUID = ri.SKUID
                                        Inner JOIN Requisitions r ON r.RequisitionID = ri.RequisitionID
                                        INNER JOIN Projects pj on pj.ProjectID = r.ProjectID
                                        Where r.ProjectID = {0} Group by r.ProjectID, ri.SKUID, sk.Name) T
                                        LEFT JOIN
                                        (Select ISNULL(po.ProjectID, 0) ProjectID, poi.SKUID, sk.Name SKUName,
                                        SUM(ISNULL(poi.TotalValue, 0)) POAmount FROM POItem poi
                                        INNER JOIN SKUs sk ON sk.SKUID = poi.SKUID
                                        INNER JOIN PurchaseOrder po ON po.POID = poi.POID
                                        INNER JOIN Projects pj on pj.ProjectID = po.ProjectID
                                        Where pj.ProjectID = {0}
                                        Group by po.ProjectID, poi.SKUID, sk.Name) T2 ON T.ProjectID = T2.ProjectID and T.SKUID = T2.SKUID
                                        LEFT JOIN
                                        (Select ISNULL(po.ProjectID, 0) ProjectID, bpi.SKUID, sk.Name SKUName,
                                        Sum(MRRNetAmount) MRRAmount, Sum(ProcessAmount) BillAmount from BillProcessItem bpi    
                                        INNER JOIN SKUs sk ON sk.SKUID = bpi.SKUID
                                        INNER JOIN MaterialReceiptReport mrr on bpi.MRRID = mrr.MRRID
                                        INNER JOIN PurchaseOrder po ON mrr.POID = po.POID
                                        Where po.ProjectID = {0}
                                        Group by po.ProjectID, bpi.SKUID, sk.Name) T3
                                        ON T.ProjectID = T3.ProjectID and T.SKUID = T3.SKUID;", projectId);
            return ExecuteDataTable();
        }

        public DataTable GetPOBreakdown(int projectId)
        {
            _sqlQuery = string.Format(@"Select T1.*,T2.MRRNetAmount,T2.ProcessAmount FROM 
                                        (Select ISNULL(po.ProjectID,0) ProjectID,poi.POID,po.SupplierID,bp.Name SupplierName,
                                        poi.SKUID,sk.Name SKUName,(poi.Rate* ISNULL(poi.RateInFC,0)) Rate,poi.OrderingQty,poi.TotalValue
                                        FROM POItem poi
                                        INNER JOIN SKUs sk ON sk.SKUID=poi.SKUID
                                        INNER JOIN PurchaseOrder po ON po.POID=poi.POID
                                        INNER JOIN Projects pj on pj.ProjectID=po.ProjectID
                                        INNER JOIN BusinessPartner bp ON bp.BPID =po.SupplierID WHERE po.ProjectID= {0}
                                        Group by po.ProjectID ,poi.POID,poi.SKUID,sk.Name,po.SupplierID,bp.Name,poi.Rate,poi.RateInFC,poi.OrderingQty,poi.TotalValue) T1
                                        LEFT JOIN
                                        (Select pj.ProjectID, po.POID,bpi.* From BillProcessItem bpi
                                        INNER JOIN MaterialReceiptReport mrr  on bpi.MRRID= mrr.MRRID
                                        INNER JOIN PurchaseOrder po  on po.POID= mrr.POID
                                        INNER JOIN Projects pj on pj.ProjectID=po.ProjectID WHERE po.ProjectID= {0} ) T2 
                                        ON T1.ProjectID=T2.ProjectID AND T1.POID=T2.POID AND T1.SKUID=T2.SKUID;", projectId);
            return ExecuteDataTable();
        }

        public DataTable GetMRRBreakdown(int projectId)
        {
            _sqlQuery = string.Format(@"SELECT T3.*,T4.ProcessAmount FROM 
                                        (Select pj.ProjectID,mrri.MRRID,po.SupplierID ,
                                         bp.Name SupplierName,mrri.SKUID,sk.Name SKUName,mrri.RateInBDT,
                                         SUM(ISNULL(mrri.MRRQuantity,0)) MRRQuantity,SUM(ISNULL(mrri.MRRAmount,0)) MRRAmount
                                         From MaterialReceiptReportItem mrri
                                         INNER JOIN SKUs sk ON sk.SKUID=mrri.SKUID
                                         INNER JOIN MaterialReceiptReport mrr ON mrr.MRRID=mrri.MRRID
                                         INNER JOIN PurchaseOrder po  on po.POID= mrr.POID
                                         INNER JOIN BusinessPartner bp ON bp.BPID =po.SupplierID
                                         INNER JOIN Projects pj on pj.ProjectID=po.ProjectID
                                         Group by pj.ProjectID, bp.BPID,mrri.MRRID ,mrri.SKUID,sk.Name,po.SupplierID,bp.Name,mrri.RateInBDT) T3
                                         LEFT JOIN (Select  bpi.MRRID,bpi.SKUID,SUM(ISNULL(bpi.ProcessAmount,0)) ProcessAmount from BillProcessItem bpi
                                         Group by  bpi.MRRID,bpi.SKUID) T4 ON T3.MRRID=T4.MRRID AND T3.SKUID=T4.SKUID 
                                         Where T3.ProjectID= {0};", projectId);
            return ExecuteDataTable();
        }

        public DataTable GetBillBreakdown(int projectId)
        {
            _sqlQuery = string.Format(@"Select T1.*,bp.Name SupplierName,T2.MRRAmount,T3.BillAmount,T5.Amount PaymentAmount FROM
                                        (Select po.ProjectID,po.SupplierID,SUM(ISNULL(poi.TotalValue,0)) POAmount 
                                        FROM POItem poi INNER JOIN PurchaseOrder po ON po.POID=poi.POID
                                        INNER JOIN Projects pj on pj.ProjectID=po.ProjectID GROUP BY po.ProjectID,po.SupplierID) T1
                                        LEFT JOIN
                                        (Select po.ProjectID,po.SupplierID,SUM(ISNULL(mrri.MRRNetAmount,0)) MRRAmount 
                                        FROM MaterialReceiptReportItem mrri INNER JOIN MaterialReceiptReport mrr ON mrr.MRRID=mrri.MRRID
                                        INNER JOIN PurchaseOrder po  on po.POID= mrr.POID
                                        INNER JOIN Projects pj on pj.ProjectID=po.ProjectID
                                        GROUP BY po.ProjectID,po.SupplierID) T2 ON T1.ProjectID=T2.ProjectID AND T1.SupplierID=T2.SupplierID
                                        LEFT JOIN
                                        (Select po.ProjectID,po.SupplierID,SUM(ISNULL(bpi.ProcessAmount,0)) BillAmount from BillProcessItem bpi
                                        INNER JOIN MaterialReceiptReport mrr ON mrr.MRRID=bpi.MRRID
                                        INNER JOIN PurchaseOrder po  on po.POID= mrr.POID
                                        INNER JOIN Projects pj on pj.ProjectID=po.ProjectID
                                        GROUP BY po.ProjectID,po.SupplierID) T3 ON T1.ProjectID=T3.ProjectID AND T1.SupplierID=T3.SupplierID
                                        LEFT JOIN
                                        (Select ISNULL(T2.SupplierID,0) SupplierID,ISNULL(T2.ProjectID,0) ProjectID,
                                        ISNULL(T2.POID,0) POID,ISNULL(T.MRRID,0) MRRID,
                                        ISNULL(bp.BillAdvanceProcessID,0) BillProcessID,p.PaymentID, p.Amount FROM Payments p
                                        INNER JOIN PaymentDetails pd ON pd.PaymentID=p.PaymentID
                                        LEFT JOIN BillAdvanceProcess bp ON bp.BillAdvanceProcessID=pd.BillAdvanceProcessID
                                        INNER JOIN
                                        (Select bpi.BillProcessID,bpi.MRRID from BillProcessItem bpi Group BY bpi.BillProcessID,bpi.MRRID) T 
                                        ON bp.BillAdvanceProcessID=T.BillProcessID
                                        INNER JOIN
                                        (Select po.POID,po.SupplierID,po.ProjectID,mrr.MRRID from  MaterialReceiptReport mrr
                                        INNER JOIN PurchaseOrder po  on po.POID= mrr.POID
                                        INNER JOIN Projects pj on pj.ProjectID=po.ProjectID) T2 ON T.MRRID=T2.MRRID
                                        ) T5 ON T1.ProjectID=T5.ProjectID AND T1.SupplierID=T5.SupplierID
                                        LEFT JOIN BusinessPartner bp ON bp.BPID =T1.SupplierID
                                        Where T3.ProjectID= {0};", projectId);
            return ExecuteDataTable();
        }

        public DataTable GetOtherBreakdown(int projectId)
        {
            _sqlQuery = string.Format(@"Select fri.FinancialRequesitionItemID,fr.FinancialRequisitionID,fr.ReferenceID ProjectID,fr.RequisitionNo,
                                        fr.RequisitionDate,Sk.SKUID,sk.Name SKUName,fri.AmountBDT From FinancialRequesitionItem fri
                                        INNER JOIN FinancialRequisition fr ON fr.FinancialRequisitionID=fri.FinancialRequisitionID
                                        INNER JOIN SKUs sk ON sk.SKUID =fri.SKUID
                                        Where ReferenceType= 4 AND ReferenceID= {0};", projectId);
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
