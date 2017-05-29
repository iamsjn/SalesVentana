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

        public DataTable GetYearlySales(int year, string reportFilters, string brandIds,
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
            string reportFilterWithAlias = this.FindReportFilterWithAlias(reportFilters);
            string reportFilterWithoutAlias = FindReportFilterWithoutAlias(reportFilters);
            string reportFilterAlias = FindReportFilterAlias(reportFilters);
            string aliasQuery = YearlyAliasQuery(reportFilterAlias);
            string orderQuery = OrderQuery(reportFilterAlias);
            string reportFilters2 = reportFilters;
            if (!string.IsNullOrEmpty(brandIds))
            {
                brandIdQuery = "AND vb.BrandID IN(" + brandIds.Trim(',') + ")";

                if (!reportFilters2.ToLower().Contains("brand"))
                {
                    reportFilters2 += ",Brand";
                }
            }

            if (!string.IsNullOrEmpty(categoryIds))
            {
                categoryIdQuery = "AND vpc.CategoryId IN(" + categoryIds.Trim(',') + ")";

                if (!reportFilters2.ToLower().Contains("productcategory"))
                {
                    reportFilters2 += ",productcategory";
                }
            }

            if (!string.IsNullOrEmpty(productIds))
            {
                productIdQuery = "AND vprod.SKUId IN(" + productIds.Trim(',') + ")";

                //if (!reportFilters2.ToLower().Contains("product"))
                //{
                    reportFilters2 += ",product";
               // }
            }

            if (!string.IsNullOrEmpty(regionIds))
            {
                regionIdQuery = "AND vr.ParentID IN(" + regionIds.Trim(',') + ")";
                if (!reportFilters2.ToLower().Contains("region"))
                {
                    reportFilters2 += ",region";
                }

            }

            if (!string.IsNullOrEmpty(channelIds))
            {
                channelIdQuery = "AND Channel.ID IN(" + channelIds.Trim(',') + ")";
                if (!reportFilters2.ToLower().Contains("channel"))
                {
                    reportFilters2 += ",channel";
                }

            }

            if (!string.IsNullOrEmpty(showroomIds))
            {
                showroomIdQuery = "AND vsr.ShowroomId IN(" + showroomIds.Trim(',') + ")";

                if (!reportFilters2.ToLower().Contains("showroom"))
                {
                    reportFilters2 += ",showroom";
                }
            }
            if (!reportFilters2.ToLower().Contains("channel"))
            {
                reportFilters2 += ",channel";
            }
            string[] searchQuery = this.FindReportFilterWithAlias2(reportFilters2);
            string fromQuery = searchQuery[0];
            string relationQuery = searchQuery[1];

            _sqlQuery = string.Format(@"declare @columns varchar(max)
                                        declare @convert varchar(max)
                                        select   @columns = stuff (( select distinct'],[' +  channel.Name 
                                        from  vw_SalesFacts sf, vw_DimChannel channel
                                        WHERE channel.ID=sf.saleschannel And sf.InvoiceDate Between '{0}' And '{1}'               
                                        for xml path('')), 1, 2, '') + ']'
                                        --print @columns
                                        set @convert =
                                        'Select t2.*,t3.InvoiceQty,'+@columns+' from(Select * from(
                                        SELECT {2},Channel.Name, SUM(vsf.NetAmount) TotalSales
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16} 
                                        {14}
                                        AND vsf.InvoiceDate BETWEEN ''{0}'' AND ''{1}'' 
                                        {5} {6} {7} {8} {9} {12}
                                        GROUP BY {3},Channel.Name)tab1
                                        pivot(sum(TotalSales) for Name
                                        in ('+@columns+')) as pivottable) t1,(
                                        select {4},cast((100*TotalSales)/NetSales as decimal(20,2))Share,TotalSales ''TotalSales(Tk)'' from (
                                        SELECT {2},SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14}
                                        AND vsf.InvoiceDate BETWEEN ''{0}'' AND ''{1}'' 
                                        {5} {6} {7} {8} {9} {12}
                                        GROUP BY {3}) tab2,
                                        (select sum(TotalSales)NetSales from(
                                        SELECT SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14} 
                                        AND vsf.InvoiceDate BETWEEN ''{0}'' AND ''{1}''   
                                        {5} {6} {7} {8} {9} {12}
                                        GROUP BY {3})tab1)tab3)t2,
                                        (SELECT {2}, SUM(vsf.InvoiceQty) InvoiceQty
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16} 
                                        {14}
                                        AND vsf.InvoiceDate BETWEEN ''{0}'' AND ''{1}'' 
                                             
                                        GROUP BY {3}
                                        ) t3 {10} {11}'
                                        execute (@convert);", startDate, endDate, reportFilterWithAlias,
                                                            reportFilterWithoutAlias, reportFilterAlias, brandIdQuery, categoryIdQuery,
                                                            productIdQuery, regionIdQuery, channelIdQuery, aliasQuery, orderQuery, showroomIdQuery, _employeeTable, _employeeSearch, fromQuery, relationQuery);

            return ExecuteDataTable();
        }

        public DataSet GetQuaterlySales(int year, string salesQuarter, string reportFilters, string brandIds,
            string categoryIds, string productIds, string regionIds, string channelIds, string showroomIds)
        {
            DataSet ds = new DataSet();
            string brandIdQuery = string.Empty;
            string categoryIdQuery = string.Empty;
            string productIdQuery = string.Empty;
            string regionIdQuery = string.Empty;
            string channelIdQuery = string.Empty;
            string showroomIdQuery = string.Empty;
            string reportFilterWithAlias = this.FindReportFilterWithAlias(reportFilters);
            string reportFilterWithoutAlias = FindReportFilterWithoutAlias(reportFilters);
            string reportFilterAlias = FindReportFilterAlias(reportFilters);
            string aliasQuery = AliasQuery(reportFilterAlias);
            string orderQuery = OrderQuery(reportFilterAlias);
            string quarterQuery = QuarterSearchQuery(year, salesQuarter.Trim(','));
            string quarterQueryCTE = QuarterSearchQueryCTE(year, salesQuarter.Trim(','));

            string reportFilters2 = reportFilters;
            if (!string.IsNullOrEmpty(brandIds))
            {
                brandIdQuery = "AND vb.BrandID IN(" + brandIds.Trim(',') + ")";

                if (!reportFilters2.ToLower().Contains("brand"))
                {
                    reportFilters2 += "Brand,";
                }
            }

            if (!string.IsNullOrEmpty(categoryIds))
            {
                categoryIdQuery = "AND vpc.CategoryId IN(" + categoryIds.Trim(',') + ")";

                if (!reportFilters2.ToLower().Contains("productcategory"))
                {
                    reportFilters2 += "productcategory,";
                }
            }

            if (!string.IsNullOrEmpty(productIds))
            {
                productIdQuery = "AND vprod.SKUId IN(" + productIds.Trim(',') + ")";

                //if (!reportFilters2.ToLower().Contains("product"))
                //{
                    reportFilters2 += "product,";
                //}
            }

            if (!string.IsNullOrEmpty(regionIds))
            {
                regionIdQuery = "AND vr.ParentID IN(" + regionIds.Trim(',') + ")";
                if (!reportFilters2.ToLower().Contains("region"))
                {
                    reportFilters2 += "region,";
                }

            }

            if (!string.IsNullOrEmpty(channelIds))
            {
                channelIdQuery = "AND Channel.ID IN(" + channelIds.Trim(',') + ")";
                if (!reportFilters2.ToLower().Contains("channel"))
                {
                    reportFilters2 += "channel,";
                }

            }

            if (!string.IsNullOrEmpty(showroomIds))
            {
                showroomIdQuery = "AND vsr.ShowroomId IN(" + showroomIds.Trim(',') + ")";

                if (!reportFilters2.ToLower().Contains("showroom"))
                {
                    reportFilters2 += "showroom,";
                }
            }
            string[] searchQuery = this.FindReportFilterWithAlias2(reportFilters2);
            string fromQuery = searchQuery[0];
            string relationQuery = searchQuery[1];

            #region Total Sales
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
                                        'SELECT t1.*,cast(t2.TotalSales as decimal(20,2)) ''TotalSales(TK)'' FROM (Select * from(SELECT {2},case DATENAME(mm, InvoiceDate) 
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
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14}
                                       
                                        {0}
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3},DATENAME(mm, vsf.InvoiceDate))tab1
                                        pivot(sum(TotalSales) for Quarterr
                                        in ('+@columns+')) as pivottable)t1,
                                        (SELECT {2},SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14}
                                       
                                        {0}                                            
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3})t2 {11} {12}  '
                                        execute (@convert);", quarterQuery, quarterQueryCTE, reportFilterWithAlias, reportFilterWithoutAlias, reportFilterAlias, brandIdQuery, categoryIdQuery,
                                                    productIdQuery, regionIdQuery, channelIdQuery, showroomIdQuery, aliasQuery, orderQuery, _employeeTable, _employeeSearch, fromQuery, relationQuery);
            ds.Tables.Add(ExecuteDataTable());
            #endregion

            #region Total Qty
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
                                        'SELECT t1.*,cast(t2.TotalSales as decimal(20,2)) ''TotalSales(TK)'' FROM (Select * from(SELECT {2},case DATENAME(mm, InvoiceDate) 
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
                                        end Quarterr,SUM(vsf.InvoiceQty) TotalSales 
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14}
                                       
                                        {0}
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3},DATENAME(mm, vsf.InvoiceDate))tab1
                                        pivot(sum(TotalSales) for Quarterr
                                        in ('+@columns+')) as pivottable)t1,
                                        (SELECT {2},SUM(vsf.InvoiceQty) TotalSales 
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14}
                                       
                                        {0}                                            
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3})t2 {11} {12}  '
                                        execute (@convert)
                                        ;", quarterQuery, quarterQueryCTE, reportFilterWithAlias, reportFilterWithoutAlias, reportFilterAlias, brandIdQuery, categoryIdQuery,
                                                    productIdQuery, regionIdQuery, channelIdQuery, showroomIdQuery, aliasQuery, orderQuery, _employeeTable, _employeeSearch, fromQuery, relationQuery);
            ds.Tables.Add(ExecuteDataTable());
            #endregion

            return ds;

        }

        public DataSet GetMonthlySales(int year, string salesMonth, string reportFilters, string brandIds,
            string categoryIds, string productIds, string regionIds, string channelIds, string showroomIds)
        {
            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = new DateTime(year, 12, 31);
            DataSet ds = new DataSet();
            string brandIdQuery = string.Empty;
            string categoryIdQuery = string.Empty;
            string productIdQuery = string.Empty;
            string regionIdQuery = string.Empty;
            string channelIdQuery = string.Empty;
            string showroomIdQuery = string.Empty;
            string reportFilterWithAlias = this.FindReportFilterWithAlias(reportFilters);
            string reportFilterWithoutAlias = FindReportFilterWithoutAlias(reportFilters);
            string reportFilterAlias = FindReportFilterAlias(reportFilters);
            string aliasQuery = AliasQuery(reportFilterAlias);
            string[] aliasQueryNew = AliasQueryNew(reportFilterAlias);
            string orderQuery = OrderQuery(reportFilterAlias);
            string monthQuery = MonthSearchQuery(year, salesMonth.Trim(','));
            string monthQueryWithWhere = MonthSearchQueryWithWhere(year, salesMonth.Trim(','));
            string prevMonthQuery = MonthSearchQuery(year-1, salesMonth.Trim(','));
            string prevMonthQueryWithWhere = MonthSearchQueryWithWhere(year - 1, salesMonth.Trim(','));
           
            string monthQueryCTE = MonthSearchQueryCTE(year, salesMonth.Trim(','));
            string reportFilters2 = reportFilters;

            if (!string.IsNullOrEmpty(brandIds))
            {
                brandIdQuery = "AND vb.BrandID IN(" + brandIds.Trim(',') + ")";

                if (!reportFilters2.ToLower().Contains("brand"))
                {
                    if (reportFilters2.LastIndexOf(',') == reportFilters2.Length-1)
                        reportFilters2 += "Brand,";
                    else
                        reportFilters2 += ",Brand";
                }
            }

            if (!string.IsNullOrEmpty(categoryIds))
            {
                categoryIdQuery = "AND vpc.CategoryId IN(" + categoryIds.Trim(',') + ")";

                if (!reportFilters2.ToLower().Contains("productcategory"))
                {
                    if (reportFilters2.LastIndexOf(',') == reportFilters2.Length - 1)
                        reportFilters2 += "productcategory,";
                    else
                        reportFilters2 += ",productcategory";
                }
            }

            if (!string.IsNullOrEmpty(productIds))
            {
                productIdQuery = "AND vprod.SKUId IN(" + productIds.Trim(',') + ")";

                //if (!reportFilters2.ToLower().Contains("product"))
                //{
                    if (reportFilters2.LastIndexOf(',') == reportFilters2.Length - 1)
                        reportFilters2 += "product,";
                    else
                        reportFilters2 += ",product";
                //}
            }

            if (!string.IsNullOrEmpty(regionIds))
            {
                regionIdQuery = "AND vr.ParentID IN(" + regionIds.Trim(',') + ")";
                if (!reportFilters2.ToLower().Contains("region"))
                {
                    if (reportFilters2.LastIndexOf(',') == reportFilters2.Length - 1)
                        reportFilters2 += "region,";
                    else
                        reportFilters2 += ",region";
                }

            }

            if (!string.IsNullOrEmpty(channelIds))
            {
                channelIdQuery = "AND Channel.ID IN(" + channelIds.Trim(',') + ")";
                if (!reportFilters2.ToLower().Contains("channel"))
                {
                    if (reportFilters2.LastIndexOf(',') == reportFilters2.Length - 1)
                        reportFilters2 += "channel,";
                    else
                        reportFilters2 += ",channel";
                }

            }

            if (!string.IsNullOrEmpty(showroomIds))
            {
                showroomIdQuery = "AND vsr.ShowroomId IN(" + showroomIds.Trim(',') + ")";

                if (!reportFilters2.ToLower().Contains("showroom"))
                {
                    if (reportFilters2.LastIndexOf(',') == reportFilters2.Length - 1)
                        reportFilters2 += "showroom,";
                    else
                        reportFilters2 += ",showroom";
                }
            }
            string[] searchQuery = this.FindReportFilterWithAlias2(reportFilters2);
            string fromQuery = searchQuery[0];
            string relationQuery = searchQuery[1];
            string columnName1 = (year-1).ToString()+ " Total";
            string columnName2 = (year-1).ToString()+ " Monthly Avg"; 
            #region Total Sales
            _sqlQuery = string.Format(@"declare @columns varchar(max)
                                        declare @convert varchar(max)
                                        declare @query varchar(max)
                                        declare @columnName1 varchar(100)
                                        declare @columnName2 varchar(100)
                                       
                                        -- CTE is needed as STUFF fails to order by correctly for varchar column
                                        ;with cte as (SELECT left(right(convert(varchar, InvoiceDate, 106), 8),3) InvoiceNumber,Row_NUMBER() OVER(ORDER BY InvoiceDate) sn
                                        from  vw_SalesFacts {1} ) 
                                        select @columns = stuff (( select '],[' +  max(InvoiceNumber)
                                        from cte sf
                                        group by invoicenumber --Generated CTE has same Month value for which its needed in group by 
                                        order by MIN (sf.sn) -- we need only one row for a single month
                                        for xml path('')), 1, 2, '') + ']'
                                        print @columns
                                        set @convert =
                                        'SELECT t1.*,cast(t2.TotalSales as decimal(20,2)) ''TotalSales(TK)'',PrevTotal ''{17}'',PrevAvg ''{18}''  FROM (Select * from(SELECT {2},left(right(convert(varchar, vsf.InvoiceDate, 106), 8),3)InvoiceDate, SUM(vsf.NetAmount) TotalSales
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14}
                                        {0}
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3} ,vsf.InvoiceDate)tab1
                                        pivot(sum(TotalSales) for InvoiceDate
                                        in ('+@columns+')) as pivottable)t1
                                        left outer join  
                                        (SELECT {2},SUM(vsf.NetAmount) TotalSales 
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14}
                                        {0}
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3}) t2 on {11}
                                        left outer join
                                        (SELECT {2},SUM(vsf.NetAmount)  PrevTotal,(SUM(vsf.NetAmount)/12) PrevAvg
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14}
                                        {19}
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3}) t3  on {20}  {12}'
                                        execute (@convert);", monthQuery, monthQueryCTE, reportFilterWithAlias, reportFilterWithoutAlias, reportFilterAlias, brandIdQuery, categoryIdQuery,
                                                                productIdQuery, regionIdQuery, channelIdQuery, showroomIdQuery, aliasQueryNew[0], orderQuery, _employeeTable, _employeeSearch, fromQuery, relationQuery, columnName1, columnName2, prevMonthQuery, aliasQueryNew[1]);

            ds.Tables.Add(ExecuteDataTable());
            #endregion

            #region Total Qty
            _sqlQuery = string.Format(@"declare @columns varchar(max)
                                        declare @convert varchar(max)
                                        -- CTE is needed as STUFF fails to order by correctly for varchar column
                                        ;with cte as (SELECT left(right(convert(varchar, InvoiceDate, 106), 8),3) InvoiceNumber,Row_NUMBER() OVER(ORDER BY InvoiceDate) sn
                                        from  vw_SalesFacts {1} ) 
                                        select @columns = stuff (( select '],[' +  max(InvoiceNumber)
                                        from cte sf
                                        group by invoicenumber --Generated CTE has same Month value for which its needed in group by 
                                        order by MIN (sf.sn) -- we need only one row for a single month
                                        for xml path('')), 1, 2, '') + ']'
                                        print @columns
                                        set @convert =
                                        'SELECT t1.*,cast(t2.TotalSales as decimal(20,2)) ''TotalQty'',PrevTotal ''{17}'',PrevAvg ''{18}'' FROM (Select * from(SELECT {2},left(right(convert(varchar, vsf.InvoiceDate, 106), 8),3)InvoiceDate, SUM(vsf.InvoiceQty) TotalSales
                                        FROM vw_SalesFacts vsf
                                       {15}
                                        {13}
                                        {16} 
                                        {14}
                                        {0}
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3} ,vsf.InvoiceDate)tab1
                                        pivot(sum(TotalSales) for InvoiceDate
                                        in ('+@columns+')) as pivottable)t1
                                         left outer join  
                                        (SELECT {2},SUM(vsf.InvoiceQty) TotalSales 
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14}
                                        {0}
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3}) t2 on {11} 
                                        left outer join
                                        (SELECT {2},SUM(vsf.InvoiceQty)  PrevTotal,(SUM(vsf.InvoiceQty)/12) PrevAvg
                                        FROM vw_SalesFacts vsf
                                        {15}
                                        {13}
                                        {16}
                                        {14}
                                        {19}
                                        {5} {6} {7} {8} {9} {10}
                                        GROUP BY {3})t3 on {20} {12}'
                                        execute (@convert);", monthQuery, monthQueryCTE, reportFilterWithAlias, reportFilterWithoutAlias, reportFilterAlias, brandIdQuery, categoryIdQuery,
                                                                productIdQuery, regionIdQuery, channelIdQuery, showroomIdQuery, aliasQueryNew[0], orderQuery, _employeeTable, _employeeSearch, fromQuery, relationQuery, columnName1, columnName2, prevMonthQuery, aliasQueryNew[1]);
            ds.Tables.Add(ExecuteDataTable());

            _sqlQuery = string.Format(@"declare @columns varchar(max)
                                        declare @convert varchar(max)
                                        -- CTE is needed as STUFF fails to order by correctly for varchar column
                                        ;with cte as (SELECT left(right(convert(varchar, InvoiceDate, 106), 8),3) InvoiceNumber,Row_NUMBER() OVER(ORDER BY InvoiceDate) sn
                                        from  vw_SalesFacts {1} ) 
                                        select @columns = stuff (( select '],[' +  max(InvoiceNumber)
                                        from cte sf
                                        group by invoicenumber --Generated CTE has same Month value for which its needed in group by 
                                        order by MIN (sf.sn) -- we need only one row for a single month
                                        for xml path('')), 1, 2, '') + ']'
                                        print @columns
                                        set @convert =
                                        'SELECT t1.*,t2.Total,PrevTotal ''{3}'',PrevAvg ''{4}'' FROM (Select * from(SELECT vsf.Description,left(right(convert(varchar, vsf.InvoiceDate, 106), 8),3)InvoiceDate, SUM(vsf.NetAmount) TotalSales
                                        FROM vw_SalesFacts vsf                                       
                                        {0}
                                        GROUP BY vsf.Description ,vsf.InvoiceDate)tab1
                                        pivot(sum(TotalSales) for InvoiceDate
                                        in ('+@columns+')) as pivottable)t1
                                        left outer join 
                                        (SELECT vsf.Description,SUM(vsf.NetAmount)  Total
                                        FROM vw_SalesFacts vsf {0}
                                        GROUP BY vsf.Description)t2
                                        on t1.Description=t2.Description
                                        left outer join 
                                        (SELECT vsf.Description,SUM(vsf.NetAmount)  PrevTotal,(SUM(vsf.NetAmount)/12) PrevAvg
                                        FROM vw_SalesFacts vsf {2}
                                        GROUP BY vsf.Description)t3
                                        on t2.Description=t3.Description'
                                        execute (@convert);", monthQueryWithWhere, monthQueryCTE,prevMonthQueryWithWhere,columnName1,columnName2);
            ds.Tables.Add(ExecuteDataTable());

            #endregion

            return ds;

        }

        private string FindReportFilterWithAlias(string reportFilters)
        {
            string reportFilter = string.Empty;
            string[] reportFilterArr = reportFilters.Split(',');

            if (reportFilterArr == null || reportFilterArr.Length <= 0)
                return reportFilter;

            for (int i = 0; i < reportFilterArr.Length; i++)
            {
                if (reportFilterArr[i].ToString().ToLower() == "brand")
                    reportFilter += "vb.Brand Brand" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "productcategory")
                    reportFilter += "vpc.Name ProductCategory" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "product")
                    reportFilter += "vprod.Name Product" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "region")
                    reportFilter += "vr.Region Region" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "showroom")
                    reportFilter += "vsr.Name Showroom" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "channel")
                    reportFilter += "channel.Name Channel" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "employee")
                    reportFilter += "emp.Name Employee" + ",";
            }

            if (!string.IsNullOrEmpty(reportFilter))
                reportFilter = reportFilter.Trim(',');

            return reportFilter;
        }

        private string[] FindReportFilterWithAlias2(string reportFilters)
        {
            string[] reportFilter = new string[2];
            string[] reportFilterArr = reportFilters.Split(',');

            if (reportFilterArr == null || reportFilterArr.Length <= 0)
                return reportFilter;
            bool bProductFound = false;
            for (int i = 0; i < reportFilterArr.Length; i++)
            {
                if (reportFilterArr[i].ToString().ToLower() == "brand")
                {
                    reportFilter[0] += "," + "vw_DimBrandSku vb";
                    if (reportFilter[1] == null)
                        reportFilter[1] += " Where vsf.SKUID=vb.SKUID  ";
                    else
                        reportFilter[1] += " AND vsf.SKUID=vb.SKUID  ";
                }
                else if (reportFilterArr[i].ToString().ToLower() == "productcategory")
                {
                    reportFilter[0] += "," + "vw_DimProductCategory vpc";
                    if (reportFilter[1] == null)
                        reportFilter[1] += " Where  vsf.categoryid=vpc.categoryid    ";
                    else
                        reportFilter[1] += " AND vsf.categoryid=vpc.categoryid    ";
                }
                else if (reportFilterArr[i].ToString().ToLower() == "product")
                {
                    if (!bProductFound)
                    {
                        reportFilter[0] += "," + "vw_DimProduct vprod";
                        if (reportFilter[1] == null)
                            reportFilter[1] += " Where vsf.SKUID=vprod.skuid  ";
                        else
                            reportFilter[1] += " AND vsf.SKUID=vprod.skuid  ";
                        bProductFound = true;
                    }
                }
                else if (reportFilterArr[i].ToString().ToLower() == "region")
                {
                    reportFilter[0] += "," + "vw_DimRegion vr";
                    if (reportFilter[1] == null)
                        reportFilter[1] += " Where  vsf.RegionID=vr.MarketHierarchyID  ";
                    else
                        reportFilter[1] += " AND vsf.RegionID=vr.MarketHierarchyID  ";
                }
                else if (reportFilterArr[i].ToString().ToLower() == "channel")
                {
                    reportFilter[0] += "," + "vw_DimChannel Channel";
                    if (reportFilter[1] == null)
                        reportFilter[1] += " Where  vsf.saleschannel=Channel.ID ";
                    else
                        reportFilter[1] += " AND vsf.saleschannel=Channel.ID  ";
                }
                else if (reportFilterArr[i].ToString().ToLower() == "showroom")
                {
                    reportFilter[0] += "," + "vw_DimShowRoom vsr";
                    if (reportFilter[1] == null)
                        reportFilter[1] += " Where  vsf.ShowroomID=vsr.showroomid ";
                    else
                        reportFilter[1] += " AND vsf.ShowroomID=vsr.showroomid ";
                }
            }


            return reportFilter;
        }

        private string FindReportFilterWithoutAlias(string reportFilters)
        {
            string reportFilter = string.Empty;
            string[] reportFilterArr = reportFilters.Split(',');

            if (reportFilterArr == null || reportFilterArr.Length <= 0)
                return reportFilter;

            for (int i = 0; i < reportFilterArr.Length; i++)
            {
                if (reportFilterArr[i].ToString().ToLower() == "brand")
                    reportFilter += "vb.Brand" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "productcategory")
                    reportFilter += "vpc.Name" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "product")
                    reportFilter += "vprod.Name" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "region")
                    reportFilter += "vr.Region" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "showroom")
                    reportFilter += "vsr.Name" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "channel")
                    reportFilter += "channel.Name" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "employee")
                    reportFilter += "emp.Name" + ",";
            }

            if (!string.IsNullOrEmpty(reportFilter))
                reportFilter = reportFilter.Trim(',');

            return reportFilter;
        }

        private string FindReportFilterAlias(string reportFilters)
        {
            string reportFilter = string.Empty;
            string[] reportFilterArr = reportFilters.Split(',');

            if (reportFilterArr == null || reportFilterArr.Length <= 0)
                return reportFilter;

            for (int i = 0; i < reportFilterArr.Length; i++)
            {
                if (reportFilterArr[i].ToString().ToLower() == "brand")
                    reportFilter += "Brand" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "productcategory")
                    reportFilter += "ProductCategory" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "product")
                    reportFilter += "Product" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "region")
                    reportFilter += "Region" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "showroom")
                    reportFilter += "Showroom" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "channel")
                    reportFilter += "Channel" + ",";
                else if (reportFilterArr[i].ToString().ToLower() == "employee")
                {
                    reportFilter += "Employee" + ",";
                    _employeeTable = ",vw_DimEmployee emp";
                    _employeeSearch = "AND vsf.salesEmployeeID=emp.EmployeeID";
                }
            }

            if (!string.IsNullOrEmpty(reportFilter))
                reportFilter = reportFilter.Trim(',');

            return reportFilter;
        }

        private string YearlyAliasQuery(string reportFilterAlias)
        {
            string aliasQuery = "Where ";
            string[] reportFilterAliasArr = reportFilterAlias.Split(',');

            for (int i = 0; i < reportFilterAliasArr.Length; i++)
            {
                if ((i + 1) < reportFilterAliasArr.Length)
                {
                    aliasQuery += "t1." + reportFilterAliasArr[i] + "=" + "t2." + reportFilterAliasArr[i] + " And " +
                        "t2." + reportFilterAliasArr[i] + "=" + "t3." + reportFilterAliasArr[i] + " And ";
                }
                else
                {
                    aliasQuery += "t1." + reportFilterAliasArr[i] + "=" + "t2." + reportFilterAliasArr[i] +
                        " And t2." + reportFilterAliasArr[i] + "=" + "t3." + reportFilterAliasArr[i];
                }
            }

            return aliasQuery;
        }

        private string AliasQuery(string reportFilterAlias)
        {
            string aliasQuery = "Where ";
            string[] reportFilterAliasArr = reportFilterAlias.Split(',');

            for (int i = 0; i < reportFilterAliasArr.Length; i++)
            {
                if ((i + 1) < reportFilterAliasArr.Length)
                {
                    aliasQuery += "t1." + reportFilterAliasArr[i] + "=" + "t2." + reportFilterAliasArr[i] + " And ";
                }
                else
                {
                    aliasQuery += "t1." + reportFilterAliasArr[i] + "=" + "t2." + reportFilterAliasArr[i] ;
                }
            }

            return aliasQuery;
        }

        private string[] AliasQueryNew(string reportFilterAlias)
        {
            string[] aliasQuery = new string[2];
            string[] reportFilterAliasArr = reportFilterAlias.Split(',');

            for (int i = 0; i < reportFilterAliasArr.Length; i++)
            {
                if ((i + 1) < reportFilterAliasArr.Length)
                {
                    aliasQuery[0] += "t1." + reportFilterAliasArr[i] + "=" + "t2." + reportFilterAliasArr[i] + " And ";
                    aliasQuery[1]+= "t2." + reportFilterAliasArr[i] + "=" + "t3." + reportFilterAliasArr[i] + " And ";
                }
                else
                {
                    aliasQuery [0]+= "t1." + reportFilterAliasArr[i] + "=" + "t2." + reportFilterAliasArr[i] ;
                    aliasQuery [1]+= "t2." + reportFilterAliasArr[i] + "=" + "t3." + reportFilterAliasArr[i];
                }
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

        private string QuarterSearchQuery(int year, string salesQuarter)
        {
            string quarterQuery = string.Empty;

            if (string.IsNullOrEmpty(salesQuarter))
                return quarterQuery = "AND vsf.InvoiceDate BETWEEN ''1 Jan " + year + "'' AND ''31 Dec " + year + "''";

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

        private string QuarterSearchQueryCTE(int year, string salesQuarter)
        {
            string quarterQuery = string.Empty;
            if (string.IsNullOrEmpty(salesQuarter))
                return quarterQuery = "Where InvoiceDate BETWEEN right(convert(varchar, '1 jan " + year + "', 106), 8) and right(convert(varchar, '31 Dec " + year + "', 106), 8)";

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

        private string MonthSearchQuery(int year, string salesMonth)
        {
            string monthQuery = string.Empty;

            if (string.IsNullOrEmpty(salesMonth))
                return monthQuery = "And vsf.InvoiceDate BETWEEN ''1 Jan " + year + "'' AND ''" + DateTime.DaysInMonth(year, 1) + " Dec " + year + "''";

            monthQuery = "AND ( ";
            string[] salesMonthArr = salesMonth.Split(',');

            for (int i = 0; i < salesMonthArr.Length; i++)
            {
                switch (salesMonthArr[i])
                {
                    case "January":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jan " + year + "'' AND ''" + DateTime.DaysInMonth(year, 1) + " Jan " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jan " + year + "'' AND ''" + DateTime.DaysInMonth(year, 1) + " Jan " + year + "''";
                        break;
                    case "February":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Feb " + year + "'' AND ''" + DateTime.DaysInMonth(year, 2) + " Feb " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Feb " + year + "'' AND ''" + DateTime.DaysInMonth(year, 2) + " Feb " + year + "''";
                        break;
                    case "March":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Mar " + year + "'' AND ''" + DateTime.DaysInMonth(year, 3) + " Mar " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Mar " + year + "'' AND ''" + DateTime.DaysInMonth(year, 3) + " Mar " + year + "''";
                        break;
                    case "April":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Apr " + year + "'' AND ''" + DateTime.DaysInMonth(year, 4) + " Apr " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Apr " + year + "'' AND ''" + DateTime.DaysInMonth(year, 4) + " Apr " + year + "''";
                        break;
                    case "May":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 May " + year + "'' AND ''" + DateTime.DaysInMonth(year, 5) + " May " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 May " + year + "'' AND ''" + DateTime.DaysInMonth(year, 5) + " May " + year + "''";
                        break;
                    case "June":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jun " + year + "'' AND ''" + DateTime.DaysInMonth(year, 6) + " Jun " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jun " + year + "'' AND ''" + DateTime.DaysInMonth(year, 6) + " Jun " + year + "''";
                        break;
                    case "July":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jul " + year + "'' AND ''" + DateTime.DaysInMonth(year, 7) + " Jul " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jul " + year + "'' AND ''" + DateTime.DaysInMonth(year, 7) + " Jul " + year + "''";
                        break;
                    case "August":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Aug " + year + "'' AND ''" + DateTime.DaysInMonth(year, 8) + " Aug " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Aug " + year + "'' AND ''" + DateTime.DaysInMonth(year, 8) + " Aug " + year + "''";
                        break;
                    case "September":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Sep " + year + "'' AND ''" + DateTime.DaysInMonth(year, 9) + " Sep " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Sep " + year + "'' AND ''" + DateTime.DaysInMonth(year, 9) + " Sep " + year + "''";
                        break;
                    case "October":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Oct " + year + "'' AND ''" + DateTime.DaysInMonth(year, 10) + " Oct " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Oct " + year + "'' AND ''" + DateTime.DaysInMonth(year, 10) + " Oct " + year + "''";
                        break;
                    case "November":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Nov " + year + "'' AND ''" + DateTime.DaysInMonth(year, 11) + " Nov " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Nov " + year + "'' AND ''" + DateTime.DaysInMonth(year, 11) + " Nov " + year + "''";
                        break;
                    case "December":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Dec " + year + "'' AND ''" + DateTime.DaysInMonth(year, 12) + " Dec " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Dec " + year + "'' AND ''" + DateTime.DaysInMonth(year, 12) + " Dec " + year + "''";
                        break;
                    default:
                        break;
                }
            }

            return monthQuery + ")";
        }

        private string MonthSearchQueryWithWhere(int year, string salesMonth)
        {
            string monthQuery = string.Empty;

            if (string.IsNullOrEmpty(salesMonth))
                return monthQuery = "Where vsf.InvoiceDate BETWEEN ''1 Jan " + year + "'' AND ''" + DateTime.DaysInMonth(year, 1) + " Dec " + year + "''";

            monthQuery = "Where ( ";
            string[] salesMonthArr = salesMonth.Split(',');

            for (int i = 0; i < salesMonthArr.Length; i++)
            {
                switch (salesMonthArr[i])
                {
                    case "January":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jan " + year + "'' AND ''" + DateTime.DaysInMonth(year, 1) + " Jan " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jan " + year + "'' AND ''" + DateTime.DaysInMonth(year, 1) + " Jan " + year + "''";
                        break;
                    case "February":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Feb " + year + "'' AND ''" + DateTime.DaysInMonth(year, 2) + " Feb " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Feb " + year + "'' AND ''" + DateTime.DaysInMonth(year, 2) + " Feb " + year + "''";
                        break;
                    case "March":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Mar " + year + "'' AND ''" + DateTime.DaysInMonth(year, 3) + " Mar " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Mar " + year + "'' AND ''" + DateTime.DaysInMonth(year, 3) + " Mar " + year + "''";
                        break;
                    case "April":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Apr " + year + "'' AND ''" + DateTime.DaysInMonth(year, 4) + " Apr " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Apr " + year + "'' AND ''" + DateTime.DaysInMonth(year, 4) + " Apr " + year + "''";
                        break;
                    case "May":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 May " + year + "'' AND ''" + DateTime.DaysInMonth(year, 5) + " May " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 May " + year + "'' AND ''" + DateTime.DaysInMonth(year, 5) + " May " + year + "''";
                        break;
                    case "June":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jun " + year + "'' AND ''" + DateTime.DaysInMonth(year, 6) + " Jun " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jun " + year + "'' AND ''" + DateTime.DaysInMonth(year, 6) + " Jun " + year + "''";
                        break;
                    case "July":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jul " + year + "'' AND ''" + DateTime.DaysInMonth(year, 7) + " Jul " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Jul " + year + "'' AND ''" + DateTime.DaysInMonth(year, 7) + " Jul " + year + "''";
                        break;
                    case "August":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Aug " + year + "'' AND ''" + DateTime.DaysInMonth(year, 8) + " Aug " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Aug " + year + "'' AND ''" + DateTime.DaysInMonth(year, 8) + " Aug " + year + "''";
                        break;
                    case "September":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Sep " + year + "'' AND ''" + DateTime.DaysInMonth(year, 9) + " Sep " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Sep " + year + "'' AND ''" + DateTime.DaysInMonth(year, 9) + " Sep " + year + "''";
                        break;
                    case "October":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Oct " + year + "'' AND ''" + DateTime.DaysInMonth(year, 10) + " Oct " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Oct " + year + "'' AND ''" + DateTime.DaysInMonth(year, 10) + " Oct " + year + "''";
                        break;
                    case "November":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Nov " + year + "'' AND ''" + DateTime.DaysInMonth(year, 11) + " Nov " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Nov " + year + "'' AND ''" + DateTime.DaysInMonth(year, 11) + " Nov " + year + "''";
                        break;
                    case "December":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Dec " + year + "'' AND ''" + DateTime.DaysInMonth(year, 12) + " Dec " + year + "''" + " OR ";
                        else
                            monthQuery += "vsf.InvoiceDate BETWEEN ''1 Dec " + year + "'' AND ''" + DateTime.DaysInMonth(year, 12) + " Dec " + year + "''";
                        break;
                    default:
                        break;
                }
            }

            return monthQuery + ")";
        }

        private string MonthSearchQueryCTE(int year, string salesMonth)
        {
            string monthQuery = string.Empty;

            if (string.IsNullOrEmpty(salesMonth))
                return monthQuery = "WHERE InvoiceDate BETWEEN right(convert(varchar, '1 Jan " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 1) + " Dec " + year + "', 106), 8)";

            monthQuery = "WHERE ( ";
            string[] salesMonthArr = salesMonth.Split(',');

            for (int i = 0; i < salesMonthArr.Length; i++)
            {
                switch (salesMonthArr[i])
                {
                    case "January":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Jan " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 1) + " Jan " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Jan " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 1) + " Jan " + year + "', 106), 8)";
                        break;
                    case "February":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Feb " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 2) + " Feb " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Feb " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 2) + " Feb " + year + "', 106), 8)";
                        break;
                    case "March":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Mar " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 3) + " Mar " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Mar " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 3) + " Mar " + year + "', 106), 8)";
                        break;
                    case "April":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Apr " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 4) + " Apr " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Apr " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 4) + " Apr " + year + "', 106), 8)";
                        break;
                    case "May":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 May " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 5) + " May " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 May " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 5) + " May " + year + "', 106), 8)";
                        break;
                    case "June":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Jun " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 6) + " Jun " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Jun " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 6) + " Jun " + year + "', 106), 8)";
                        break;
                    case "July":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Jul " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 7) + " Jul " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Jul " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 7) + " Jul " + year + "', 106), 8)";
                        break;
                    case "August":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Aug " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 8) + " Aug " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Aug " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 8) + " Aug " + year + "', 106), 8)";
                        break;
                    case "September":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Sep " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 9) + " Sep " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Sep " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 9) + " Sep " + year + "', 106), 8)";
                        break;
                    case "October":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Oct " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 10) + " Oct " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Oct " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 10) + " Oct " + year + "', 106), 8)";
                        break;
                    case "November":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Nov " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 11) + " Nov " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Nov " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 11) + " Nov " + year + "', 106), 8)";
                        break;
                    case "December":
                        if ((i + 1) < salesMonthArr.Length)
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Dec " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 12) + " Dec " + year + "', 106), 8)" + " OR ";
                        else
                            monthQuery += "InvoiceDate BETWEEN right(convert(varchar, '1 Dec " + year + "', 106), 8) and right(convert(varchar, '" + DateTime.DaysInMonth(year, 12) + " Dec " + year + "', 106), 8)";
                        break;
                    default:
                        break;
                }
            }

            return monthQuery + ")";
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
