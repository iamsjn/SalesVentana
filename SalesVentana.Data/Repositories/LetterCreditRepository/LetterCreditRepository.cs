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
    public class LetterCreditRepository : ILetterCreditRepository
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

        public LetterCreditRepository(IDbFactory dbFactory)
        {
            DbFactory = dbFactory;
        }
        #endregion
        public DataTable GetStatus()
        {
            _sqlQuery = string.Format(@"Select LCStatusId StatusId, Name StatusName From vw_DimLCStatus;");
            return ExecuteDataTable();
        }

        public DataTable GetBank()
        {
            _sqlQuery = string.Format(@"Select BankId BankId, BankName BankName From vw_DimBank;");
            return ExecuteDataTable();
        }

        public DataTable GetSupplier()
        {
            _sqlQuery = string.Format(@"Select SupplierId SupplierId, SupplierName SupplierName From vw_DimSupplier;");
            return ExecuteDataTable();
        }

        public DataTable GetTerm()
        {
            _sqlQuery = string.Format(@"Select PaymentOptionId TermId, Name TermName From vw_DimLCTermsofPaymentOption;");
            return ExecuteDataTable();
        }

        public DataTable GetLCSummary()
        {
            _sqlQuery = string.Format(@"SELECT lcf.lcid, lcf.lcno 'LC no.',CONVERT(VARCHAR(11),lcf.issuedate,106) 'Issue Date',lcf.lcvalue 'LC Value',cur.Currency Currency,lcf.lcvaluetk 'LC Value(TK.)',
                                        lcitem.itemcount 'Number of Items',bank.BankName 'Own Bank',supplier.SupplierName 'Supplier',
                                        lastact.lastactivity 'Last Activity Done',CONVERT(VARCHAR(11),lastact.activityDate ,106)'Activity Date',payoption.Name 'Payment Terms',
                                        CONVERT(VARCHAR(11),lcf.expiredate,106)  'Retirement Date'
                                        FROM vw_LCFact lcf 
                                        LEFT OUTER JOIN vw_DimCurrency cur ON lcf.currencyid=cur.currencyID
                                        LEFT OUTER JOIN vw_DimBank bank ON lcf.ownbankid=bank.BankID
                                        LEFT OUTER JOIN vw_DimSupplier supplier ON lcf.supplierid=supplier.SupplierID
                                        LEFT OUTER JOIN vw_DimLastActivity lastact ON lcf.lcid=lastact.lcid
                                        LEFT OUTER JOIN vw_DimLCItemsCount lcitem ON lcitem.lcid=lcf.lcid
                                        LEFT OUTER JOIN vw_DimLCTermsofPaymentOption payoption ON payoption.PaymentOptionID=lcf.Termsofpaymentoption
                                        LEFT OUTER JOIN vw_DimLCStatus lcstatus ON lcstatus.LCStatusID=lcf.lcstatus
                                        WHERE lcf.issuedate BETWEEN '1 jan 2017' AND '30 Apr 2017'
                                        AND lcf.lcstatus=2
                                        AND lcf.supplierid=3351
                                        AND lcf.ownbankid=57
                                        AND lcf.Termsofpaymentoption=0
                                        ORDER BY lcf.issuedate;");
            return ExecuteDataTable();
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
