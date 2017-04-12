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
                categoryIdQuery = "Where ParentId In (" + categoryIds + ")";

            _sqlQuery = string.Format(@"SELECT SKUId ProductId, p1.Name ProductName FROM vw_DimProduct p1 {0};", categoryIdQuery);
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

        public DataTable GetShowroom()
        {
            _sqlQuery = string.Format(@"Select * from vw_DimShowRoom;");
            return ExecuteDataTable();
        }

        public DataTable GetProductCategory(string brandIds)
        {
            string brandIdQuery = string.Empty;
            if (!string.IsNullOrEmpty(brandIds))
                brandIdQuery = "And BS.BrandId In (" + brandIds + ")";

            _sqlQuery = string.Format(@"SELECT Distinct CategoryId, PC.Name CategoryName FROM vw_DimProductCategory PC
                                        Where parentcategoryid IS NULL ORDER BY name;", brandIdQuery);
            return ExecuteDataTable();
        }

        public DataTable GetYearlySales(int year, string reportType, string brandIds, string categoryIds, string productIds,
            string regionIds, string channelIds, string showroomIds)
        {
            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = new DateTime(year, 12, 31);
            string brandIdQuery = string.Empty;
            string categoryIdQuery = string.Empty;
            string productIdQuery = string.Empty;
            string regionIdQuery = string.Empty;
            string channelIdQuery = string.Empty;
            string showroomIdQuery = string.Empty;
            string reportFilterWithAlias = this.FindReportFilterWithAlias(reportType);
            string reportFilterWithoutAlias = FindReportFilterWithoutAlias(reportType);
            string reportFilterAlias = FindReportFilterAlias(reportType);
            string aliasQuery = AliasQuery(reportFilterAlias);
            string orderQuery = OrderQuery(reportFilterAlias);

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

            if (!string.IsNullOrEmpty(showroomIds))
                showroomIdQuery = "AND vsr.ShowroomId IN(" + showroomIds.Trim(',') + ")";

            _sqlQuery = string.Format(@"declare @columns varchar(max)
                                        declare @convert varchar(max)
                                        select   @columns = stuff (( select distinct'],[' +  channel.Name 
                                        from  vw_SalesFacts sf, vw_DimChannel channel
                                        WHERE channel.ID=sf.saleschannel And sf.InvoiceDate Between '{0}' And '{1}'               
                                        for xml path('')), 1, 2, '') + ']'
                                        --print @columns
                                        set @convert =
                                        'Select t2.*,'+@columns+' from(Select * from(
                                        SELECT {2},Channel.Name,SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf,
                                        vw_DimBrandSku vb,
                                        vw_DimProduct vprod,
                                        vw_DimProductCategory vpc,
                                        vw_DimRegion vr,
                                        vw_DimShowRoom vsr,
                                        vw_DimChannel Channel
                                        WHERE vsf.SKUID=vb.SKUID
                                        AND vprod.skuid=vsf.SKUID
                                        AND vprod.ParentId=vpc.categoryid  
                                        AND vsf.saleschannel=Channel.ID  
                                        AND vsf.ShowroomID=vsr.showroomid
                                        AND vsr.RegionID=vr.MarketHierarchyID 
                                        AND vsf.InvoiceDate BETWEEN ''{0}'' AND ''{1}'' 
                                        {5} {6} {7} {8} {9} {12}
                                        GROUP BY {3},Channel.Name)tab1
                                        pivot(sum(TotalSales) for Name
                                        in ('+@columns+')) as pivottable) t1,(
                                        select {4},FORMAT((100*TotalSales)/NetSales,''N2'')Share,TotalSales ''Total Sales(Tk)'',FORMAT(TotalSales/1000000,''N2'')''Total Sales(Tk M)'' from (
                                        SELECT {2},SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf,
                                        vw_DimBrandSku vb,  
                                        vw_DimProduct vprod,
                                        vw_DimProductCategory vpc,
                                        vw_DimRegion vr,
                                        vw_DimShowRoom vsr,
                                        vw_DimChannel Channel
                                        WHERE vsf.SKUID=vb.SKUID
                                        AND vprod.skuid=vsf.SKUID
                                        AND vprod.ParentId=vpc.categoryid  
                                        AND vsf.saleschannel=Channel.ID  
                                        AND vsf.ShowroomID=vsr.showroomid
                                        AND vsr.RegionID=vr.MarketHierarchyID 
                                        AND vsf.InvoiceDate BETWEEN ''{0}'' AND ''{1}'' 
                                        {5} {6} {7} {8} {9} {12}
                                        GROUP BY {3}) tab2,
                                        (select sum(TotalSales)NetSales from(
                                        SELECT SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf,
                                        vw_DimBrandSku vb,  
                                        vw_DimProduct vprod,
                                        vw_DimProductCategory vpc,
                                        vw_DimRegion vr,
                                        vw_DimShowRoom vsr,
                                        vw_DimChannel Channel
                                        WHERE vsf.SKUID=vb.SKUID
                                        AND vprod.skuid=vsf.SKUID
                                        AND vprod.ParentId=vpc.categoryid  
                                        AND vsf.saleschannel=Channel.ID  
                                        AND vsf.ShowroomID=vsr.showroomid
                                        AND vsr.RegionID=vr.MarketHierarchyID 
                                        AND vsf.InvoiceDate BETWEEN ''{0}'' AND ''{1}''   
                                        {5} {6} {7} {8} {9} {12}
                                        GROUP BY {3})tab1)tab3)t2 {10} {11}'
                                        execute (@convert);", startDate, endDate, reportFilterWithAlias, 
                                                            reportFilterWithoutAlias, reportFilterAlias, brandIdQuery, categoryIdQuery, 
                                                            productIdQuery, regionIdQuery, channelIdQuery, aliasQuery, orderQuery, showroomIdQuery);
            return ExecuteDataTable();
        }

        private string FindReportFilterWithAlias(string reportType)
        {
            string reportFilter = string.Empty;
            string[] reportTypeArr = reportType.Split(',');

            if (reportTypeArr == null || reportTypeArr.Length <= 0)
                return reportFilter;

            for (int i = 0; i < reportTypeArr.Length; i++)
            {
                if (reportTypeArr[i].ToString().ToLower() == "brandtype:true")
                    reportFilter += "vb.Brand Brand" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "categorytype:true")
                    reportFilter += "vpc.Name ProductCategory" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "producttype:true")
                    reportFilter += "vprod.Name Product" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "regiontype:true")
                    reportFilter += "vr.Region Region" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "showroomtype:true")
                    reportFilter += "vsr.Name Showroom" + ",";
            }

            if (!string.IsNullOrEmpty(reportFilter))
                reportFilter = reportFilter.Trim(',');

            return reportFilter;
        }

        private string FindReportFilterWithoutAlias(string reportType)
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
                else if (reportTypeArr[i].ToString().ToLower() == "regiontype:true")
                    reportFilter += "vr.Region" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "showroomtype:true")
                    reportFilter += "vsr.Name" + ",";
            }

            if (!string.IsNullOrEmpty(reportFilter))
                reportFilter = reportFilter.Trim(',');

            return reportFilter;
        }

        private string FindReportFilterAlias(string reportType)
        {
            string reportFilter = string.Empty;
            string[] reportTypeArr = reportType.Split(',');

            if (reportTypeArr == null || reportTypeArr.Length <= 0)
                return reportFilter;

            for (int i = 0; i < reportTypeArr.Length; i++)
            {
                if (reportTypeArr[i].ToString().ToLower() == "brandtype:true")
                    reportFilter += "Brand" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "categorytype:true")
                    reportFilter += "ProductCategory" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "producttype:true")
                    reportFilter += "Product" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "regiontype:true")
                    reportFilter += "Region" + ",";
                else if (reportTypeArr[i].ToString().ToLower() == "showroomtype:true")
                    reportFilter += "Showroom" + ",";
            }

            if (!string.IsNullOrEmpty(reportFilter))
                reportFilter = reportFilter.Trim(',');

            return reportFilter;
        }

        private string AliasQuery(string reportFilterAlias)
        {
            string aliasQuery = "Where ";
            string[] reportFilterAliasArr = reportFilterAlias.Split(',');

            for (int i = 0; i < reportFilterAliasArr.Length; i++)
            {
                if ((i + 1) < reportFilterAliasArr.Length)
                    aliasQuery += "t1." + reportFilterAliasArr[i] + "=" + "t2." + reportFilterAliasArr[i] + " And ";
                else
                    aliasQuery += "t1." + reportFilterAliasArr[i] + "=" + "t2." + reportFilterAliasArr[i];
            }

            return aliasQuery;
        }

        private string OrderQuery(string reportFilterAlias)
        {
            string aliasQuery = "Order By ";
            string[] reportFilterAliasArr = reportFilterAlias.Split(',');

            for (int i = 0; i < reportFilterAliasArr.Length; i++)
            {
                if ((i + 1) < reportFilterAliasArr.Length)
                    aliasQuery += "t1." + reportFilterAliasArr[i]  + " , ";
                else
                    aliasQuery += "t1." + reportFilterAliasArr[i];
            }

            return aliasQuery;
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
