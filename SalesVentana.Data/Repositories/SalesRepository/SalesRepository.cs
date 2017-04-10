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
    public class SalesRepository : ISalesRepository
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
        public SalesRepository(IDbFactory dbFactory)
        {
            DbFactory = dbFactory;
        }
        #endregion
        public DataTable GetBrand()
        {
            _sqlQuery = string.Format(@"Select Id BrandID, Name BrandName From Brand;");
            return ExecuteDataTable();
        }

        public DataTable GetProduct(string categoryIds)
        {
            string categoryIdQuery = string.Empty;
            if (!string.IsNullOrEmpty(categoryIds))
                categoryIdQuery = "AND CategoryId In (" + categoryIds + ")";

            _sqlQuery = string.Format(@"SELECT SKUId ProductId, p1.Name ProductName FROM vw_DimProduct p1, vw_DimProductCategory p2 WHERE p1.Hierarchynodeid = p2.categoryid {0};", categoryIdQuery);
            return ExecuteDataTable();
        }

        public DataTable GetProductCategory(string brandIds)
        {
            string brandIdQuery = string.Empty;
            if (!string.IsNullOrEmpty(brandIds))
                brandIdQuery = "And BS.BrandId In (" + brandIds + ")";

            _sqlQuery = string.Format(@"SELECT Distinct CategoryId, PC.Name CategoryName FROM vw_DimBrandSku BS, vw_DimProduct Prod, vw_DimProductCategory PC
                                        Where BS.SKUID=Prod.SKUID
                                        And Prod.HierarchynodeId=PC.CategoryId {0};", brandIdQuery);
            return ExecuteDataTable();
        }

        public DataTable GetYearlySales(int year)
        {
            _sqlQuery = string.Format(@"select Brand,FORMAT((100*TotalSales)/NetSales,'N2')Share,TotalSales 'Total Sales(Tk)',FORMAT(TotalSales/1000000,'N2')'Total Sales(Tk M)' from (
                                        SELECT vb.Brand,SUM(vsf.NetAmount) TotalSales FROM vw_SalesFacts vsf,
                                        vw_DimBrandSku vb, vw_DimProduct vprod, vw_DimProductCategory vpc, vw_DimRegion vr, vw_DimShowRoom vsr, vw_DimChannel Channel
                                        WHERE vsf.SKUID=vb.SKUID AND vprod.skuid=vsf.SKUID AND vprod.Hierarchynodeid=vpc.categoryid  
                                        AND vsf.saleschannel=Channel.ID AND vsf.ShowroomID=vsr.showroomid AND vsr.RegionID=vr.MarketHierarchyID 
                                        AND (vsf.InvoiceDate BETWEEN '1 Jan 2017' AND '31 Jan 2017'
                                        OR vsf.InvoiceDate BETWEEN '1 Mar 2017' AND '31 Mar 2017'
                                        OR vsf.InvoiceDate BETWEEN '1 Feb 2017' AND '28 Feb 2017')   
                                        --AND vb.BrandID IN() -- Brand Filter
                                        --AND vpc.categoryid IN() -- Product Category Filter
                                        --AND vprod.skuid IN()  -- Product Filter
                                        --AND Channel.ID IN() -- Channel Filter
                                        --AND vr.MarketHierarchyID IN()   -- Region Filter
                                        GROUP BY vb.Brand) tab2, (select sum(TotalSales)NetSales from(
                                        SELECT vb.Brand,SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf, vw_DimBrandSku vb, vw_DimProduct vprod, vw_DimProductCategory vpc,
                                        vw_DimRegion vr, vw_DimShowRoom vsr, vw_DimChannel Channel
                                        WHERE vsf.SKUID=vb.SKUID AND vprod.skuid=vsf.SKUID AND vprod.Hierarchynodeid=vpc.categoryid  
                                        AND vsf.saleschannel=Channel.ID AND vsf.ShowroomID=vsr.showroomid AND vsr.RegionID=vr.MarketHierarchyID AND (vsf.InvoiceDate BETWEEN '1 Jan 2017' AND '31 Jan 2017'
                                        OR vsf.InvoiceDate BETWEEN '1 Mar 2017' AND '31 Mar 2017' OR vsf.InvoiceDate BETWEEN '1 Feb 2017' AND '28 Feb 2017')    
                                        -- AND vb.BrandID IN() -- Brand Filter
                                        --AND vpc.categoryid IN() -- Product Category Filter
                                        --AND vprod.skuid IN()  -- Product Filter
                                        -- AND Channel.ID IN() -- Channel Filter
                                        -- AND vr.MarketHierarchyID IN() -- Region Filter 
                                        GROUP BY vb.Brand)tab1)tab3;");
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
