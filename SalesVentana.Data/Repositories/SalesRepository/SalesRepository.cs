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

        public DataTable GetYearlySales(int year, string reportType, string brandIds,
            string categoryIds, string productIds, string regionIds, string channelIds, string showroomIds)
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
                                        SELECT {2},Channel.Name, SUM(vsf.NetAmount) TotalSales
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
                                        select {4},cast((100*TotalSales)/NetSales as decimal(20,2))Share,TotalSales ''TotalSales(Tk)'',cast(TotalSales/1000000 as decimal(20,2))''TotalSales(Mil)'' from (
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

        public DataTable GetQuaterlySales(int year, string salesQuarter, string reportType, string brandIds,
            string categoryIds, string productIds, string regionIds, string channelIds, string showroomIds)
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
            string quarterQuery = QuarterSearchQuery(year, salesQuarter.Trim(','));
            string quarterQueryCTE = QuarterSearchQueryCTE(year, salesQuarter.Trim(','));

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
                                        declare @convert varchar(max);
                                        with cte as (SELECT  
                                        case DATENAME(mm, InvoiceDate) 
                                        when 'January' then 'Q1' 
                                        when 'February' then 'Q1' 
                                        when 'March' then 'Q1' 
                                        when 'April' then 'Q2' 
                                        when 'May' then 'Q2' 
                                        when 'June' then 'Q2' 
                                        when 'July' then 'Q3' 
                                        when 'August' then 'Q3' 
                                        when 'September' then 'Q3' 
                                        when 'October' then 'Q4' 
                                        when 'November' then 'Q4' 
                                        when 'December' then 'Q4' 
                                        end InvoiceNumber
                                        ,Row_NUMBER() OVER(ORDER BY InvoiceDate) sn
                                        from  vw_SalesFacts 
                                        {1}
                                        ) select @columns = stuff (( select '],[' +  max(InvoiceNumber)
                                        from cte sf
                                        group by invoicenumber --Generated CTE has same Month value for which its needed in group by 
                                        order by MIN (sf.sn) -- we need only one row for a single month
                                        for xml path('')), 1, 2, '') + ']'
                                        print @columns
                                        set @convert =
                                        'SELECT t1.*,cast(t2.TotalSales as decimal(20,2)) ''TotalSales(TK)'',cast(t2.TotalSales/1000000 as decimal(20,2)) ''TotalSales(Mil)''  FROM (Select * from(SELECT {2},case DATENAME(mm, InvoiceDate) 
                                        when ''January'' then ''Q1''
                                        when ''February'' then ''Q1''
                                        when ''March'' then ''Q1''
                                        when ''April'' then ''Q2''
                                        when ''May'' then ''Q2''
                                        when ''June'' then ''Q2''
                                        when ''July'' then ''Q3''
                                        when ''August'' then ''Q3'' 
                                        when ''September'' then ''Q3'' 
                                        when ''October'' then ''Q4''
                                        when ''November'' then ''Q4''
                                        when ''December'' then ''Q4''
                                        end Quarterr,SUM(vsf.NetAmount) TotalSales 
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
                                        {0}
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3},DATENAME(mm, vsf.InvoiceDate))tab1
                                        pivot(sum(TotalSales) for Quarterr
                                        in ('+@columns+')) as pivottable)t1,
                                        (SELECT {2},SUM(vsf.NetAmount) TotalSales 
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
                                        {0}                                            
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3})t2 {11} {12}  '
                                        execute (@convert)
                                        ;", quarterQuery, quarterQueryCTE, reportFilterWithAlias, reportFilterWithoutAlias, reportFilterAlias, brandIdQuery, categoryIdQuery,
                                                            productIdQuery, regionIdQuery, channelIdQuery, showroomIdQuery, aliasQuery, orderQuery);
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
                    aliasQuery += "t1." + reportFilterAliasArr[i] + " , ";
                else
                    aliasQuery += "t1." + reportFilterAliasArr[i];
            }

            return aliasQuery;
        }

        private string QuarterSearchQuery( int year, string salesQuarter)
        {
            string quarterQuery = string.Empty;

            if (string.IsNullOrEmpty(salesQuarter))
                return quarterQuery;

            quarterQuery = "AND ( ";
            string[] salesQuarterArr = salesQuarter.Split(',');

            for (int i = 0; i < salesQuarterArr.Length; i++)
            {
                switch (salesQuarterArr[i])
                {
                    case "Q1":
                        if ((i + 1) < salesQuarterArr.Length)
                            quarterQuery += "vsf.InvoiceDate BETWEEN ''1 Jan " + year + "'' AND ''31 mar " + year + "''" + " OR ";
                        else
                            quarterQuery += "vsf.InvoiceDate BETWEEN ''1 Jan " + year + "'' AND ''31 mar " + year + "''";
                        break;
                    case "Q2":
                        if ((i + 1) < salesQuarterArr.Length)
                            quarterQuery += "vsf.InvoiceDate BETWEEN ''1 apr " + year + "'' AND ''30 jun " + year + "''" + " OR ";
                        else
                            quarterQuery += "vsf.InvoiceDate BETWEEN ''1 apr " + year + "'' AND ''30 jun " + year + "''";
                        break;
                    case "Q3":
                        if ((i + 1) < salesQuarterArr.Length)
                            quarterQuery += "vsf.InvoiceDate BETWEEN ''1 jul " + year + "'' AND ''30 sep " + year + "''" + " OR ";
                        else
                            quarterQuery += "vsf.InvoiceDate BETWEEN ''1 jul " + year + "'' AND ''30 sep " + year + "''";
                        break;
                    case "Q4":
                        if ((i + 1) < salesQuarterArr.Length)
                            quarterQuery += "vsf.InvoiceDate BETWEEN ''1 oct " + year + "'' AND ''31 dec " + year + "''" + " OR ";
                        else
                            quarterQuery += "vsf.InvoiceDate BETWEEN ''1 oct " + year + "'' AND ''31 dec " + year + "''";
                        break;
                    default:
                        break;
                }
            }

            return quarterQuery + ")";
        }

        private string QuarterSearchQueryCTE( int year, string salesQuarter)
        {
            string quarterQuery = string.Empty;
            if(string.IsNullOrEmpty(salesQuarter))
                return quarterQuery;

            quarterQuery = "WHERE ( ";
            string[] salesQuarterArr = salesQuarter.Split(',');

            for (int i = 0; i < salesQuarterArr.Length; i++)
            {
                switch (salesQuarterArr[i])
                {
                    case "Q1":
                        if ((i + 1) < salesQuarterArr.Length)
                            quarterQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 jan " + year + "', 106), 8) and right(convert(varchar, '31 mar " + year + "', 106), 8)" + " OR ";
                        else
                            quarterQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 jan " + year + "', 106), 8) and right(convert(varchar, '31 mar " + year + "', 106), 8)";
                        break;
                    case "Q2":
                        if ((i + 1) < salesQuarterArr.Length)
                            quarterQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 apr " + year + "', 106), 8) and right(convert(varchar, '30 jun " + year + "', 106), 8)" + " OR ";
                        else
                            quarterQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 apr " + year + "', 106), 8) and right(convert(varchar, '30 jun " + year + "', 106), 8)";
                        break;
                    case "Q3":
                        if ((i + 1) < salesQuarterArr.Length)
                            quarterQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 jul " + year + "', 106), 8) and right(convert(varchar, '30 sep " + year + "', 106), 8)" + " OR ";
                        else
                            quarterQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 jul " + year + "', 106), 8) and right(convert(varchar, '30 sep " + year + "', 106), 8)";
                        break;
                    case "Q4":
                        if ((i + 1) < salesQuarterArr.Length)
                            quarterQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 oct " + year + "', 106), 8) and right(convert(varchar, '31 dec " + year + "', 106), 8)" + " OR ";
                        else
                            quarterQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 oct " + year + "', 106), 8) and right(convert(varchar, '31 dec " + year + "', 106), 8)";
                        break;
                    default:
                        break;
                }
            }

            return quarterQuery + ")";
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
