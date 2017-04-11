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

        public DataTable GetRegion()
        {
            _sqlQuery = string.Format(@"SELECT Distinct MarketHierarchyID RegionId, Region RegionName From vw_DimRegion Where ParentId IS Null;");
            return ExecuteDataTable();
        }

        public DataTable GetChannel()
        {
            _sqlQuery = string.Format(@"SELECT Distinct Id ChannelId, Name ChannelName From vw_DimChannel;");
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

        public DataTable GetYearlySales(int year, string reportType, string brandIds, string categoryIds, string productIds,
            string regionIds, string channelIds)
        {
            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = new DateTime(year, 12, 31);
            string brandIdQuery = string.Empty;
            string categoryIdQuery = string.Empty;
            string productIdQuery = string.Empty;
            string regionIdQuery = string.Empty;
            string channelIdQuery = string.Empty;
            string reportFilter = this.FindReportFilter(reportType);
            string reportFilterAlias = "Item";

            if (!string.IsNullOrEmpty(brandIds))
                brandIdQuery = "AND vb.BrandID IN(" + brandIds.Trim(',') + ")";

            if (!string.IsNullOrEmpty(categoryIds))
                categoryIdQuery = "AND vpc.CategoryId IN(" + categoryIds.Trim(',') + ")";

            if (!string.IsNullOrEmpty(productIds))
                productIdQuery = "AND vprod.SKUId IN(" + productIds.Trim(',') + ")";

            if (!string.IsNullOrEmpty(regionIds))
                regionIdQuery = "AND vr.ParentID IN(" + regionIds.Trim(',') + ")";

            if (!string.IsNullOrEmpty(channelIds))
                channelIdQuery = "AND Channel.ID IN(" + channelIds.Trim(',') + ")";

            _sqlQuery = string.Format(@"declare @columns varchar(max)
                                        declare @convert varchar(max)
                                        select   @columns = stuff (( select distinct'],[' +  channel.Name 
                                        from  vw_SalesFacts sf, vw_DimChannel channel
                                        WHERE channel.ID=sf.saleschannel And sf.InvoiceDate Between '{0}' And '{1}'               
                                        for xml path('')), 1, 2, '') + ']'
                                        --print @columns
                                        set @convert =
                                        'Select t2.*,'+@columns+' from(Select * from(
                                        SELECT {2} {3},Channel.Name,SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf,
                                        vw_DimBrandSku vb,
                                        vw_DimProduct vprod,
                                        vw_DimProductCategory vpc,
                                        vw_DimRegion vr,
                                        vw_DimShowRoom vsr,
                                        vw_DimChannel Channel
                                        WHERE vsf.SKUID=vb.SKUID
                                        AND vprod.skuid=vsf.SKUID
                                        AND vprod.Hierarchynodeid=vpc.categoryid  
                                        AND vsf.saleschannel=Channel.ID  
                                        AND vsf.ShowroomID=vsr.showroomid
                                        AND vsr.RegionID=vr.MarketHierarchyID 
                                        AND vsf.InvoiceDate BETWEEN ''{0}'' AND ''{1}'' 
                                        {4} {5} {6} {7} {8}
                                        GROUP BY {2},Channel.Name)tab1
                                        pivot(sum(TotalSales) for Name
                                        in ('+@columns+')) as pivottable) t1,(
                                        select {3},FORMAT((100*TotalSales)/NetSales,''N2'')Share,TotalSales ''Total Sales(Tk)'',FORMAT(TotalSales/1000000,''N2'')''Total Sales(Tk M)'' from (
                                        SELECT {2} {3},SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf,
                                        vw_DimBrandSku vb,  
                                        vw_DimProduct vprod,
                                        vw_DimProductCategory vpc,
                                        vw_DimRegion vr,
                                        vw_DimShowRoom vsr,
                                        vw_DimChannel Channel
                                        WHERE vsf.SKUID=vb.SKUID
                                        AND vprod.skuid=vsf.SKUID
                                        AND vprod.Hierarchynodeid=vpc.categoryid  
                                        AND vsf.saleschannel=Channel.ID  
                                        AND vsf.ShowroomID=vsr.showroomid
                                        AND vsr.RegionID=vr.MarketHierarchyID 
                                        AND vsf.InvoiceDate BETWEEN ''{0}'' AND ''{1}'' 
                                        {4} {5} {6} {7} {8}
                                        GROUP BY {2}) tab2,
                                        (select sum(TotalSales)NetSales from(
                                        SELECT {2} {3},SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf,
                                        vw_DimBrandSku vb,  
                                        vw_DimProduct vprod,
                                        vw_DimProductCategory vpc,
                                        vw_DimRegion vr,
                                        vw_DimShowRoom vsr,
                                        vw_DimChannel Channel
                                        WHERE vsf.SKUID=vb.SKUID
                                        AND vprod.skuid=vsf.SKUID
                                        AND vprod.Hierarchynodeid=vpc.categoryid  
                                        AND vsf.saleschannel=Channel.ID  
                                        AND vsf.ShowroomID=vsr.showroomid
                                        AND vsr.RegionID=vr.MarketHierarchyID 
                                        AND vsf.InvoiceDate BETWEEN ''{0}'' AND ''{1}''   
                                        {4} {5} {6} {7} {8}
                                        GROUP BY {2})tab1)tab3)t2 where t1.{3}=t2.{3} order by t1.{3}'
                                        execute (@convert);", startDate, endDate, reportFilter, reportFilterAlias, brandIdQuery, categoryIdQuery, productIdQuery, regionIdQuery, channelIdQuery);
            return ExecuteDataTable();
        }

        private string FindReportFilter(string reportType)
        {
            string reportFilter = string.Empty;
            string[] reportTypeArr = reportType.Split(',');

            if (reportTypeArr == null || reportTypeArr.Length <= 0)
                return reportFilter;

            for (int i = 0; i < reportTypeArr.Length; i++)
            {
                if (reportTypeArr[i].ToString().ToLower() == "brandtype:true")
                    reportFilter += "vb.Brand" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "categorytype:true")
                    reportFilter += "vpc.Name" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "producttype:true")
                    reportFilter += "vprod.Name" + ",";
            }

            if (!string.IsNullOrEmpty(reportFilter))
                reportFilter = reportFilter.Trim(',');

            return reportFilter;
        }

        private string FindReportFilterAlias(string reportType)
        {
            string reportFilterAlias = string.Empty;
            switch (reportType)
            {
                case "1":
                    reportFilterAlias = "Item";
                    return reportFilterAlias;
                case "2":
                    reportFilterAlias = "Item";
                    return reportFilterAlias;
                case "3":
                    reportFilterAlias = "Item";
                    return reportFilterAlias;
                default:
                    return reportFilterAlias;
            }
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
