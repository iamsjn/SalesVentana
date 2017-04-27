using Newtonsoft.Json.Linq;
using SalesVentana.BO;
using SalesVentana.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SalesVentana.Controllers
{
    [Authorize]
    [RoutePrefix("api/sales")]
    public class SalesController : ApiControllerBase
    {
        ISalesRepository _salesRepository = null;
        IUnitOfWork _unitOfWork = null;
        public SalesController(IBaseRepository<Error> errorRepository, ISalesRepository salesRepository,
            IUnitOfWork unitOfWork)
            : base(errorRepository, unitOfWork)
        {
            _salesRepository = salesRepository;
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [Route("brand")]
        [HttpGet]
        public HttpResponseMessage GetBrand(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _salesRepository.GetBrand();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    brands = table.AsEnumerable().Select(x => new { brandId = x.Field<int>("BrandId"), brandName = x.Field<string>("BrandName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("region")]
        [HttpGet]
        public HttpResponseMessage GetRegion(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _salesRepository.GetRegion();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    regions = table.AsEnumerable().Select(x => new { regionId = x.Field<int>("RegionId"), regionName = x.Field<string>("RegionName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("channel")]
        [HttpGet]
        public HttpResponseMessage GetChannel(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _salesRepository.GetChannel();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    channels = table.AsEnumerable().Select(x => new { channelId = x.Field<int>("ChannelId"), channelName = x.Field<string>("ChannelName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("showroom")]
        [HttpGet]
        public HttpResponseMessage GetShowroom(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _salesRepository.GetShowroom();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    showrooms = table.AsEnumerable().Select(x => new { showroomId = x.Field<int>("ShowroomId"), showroomName = x.Field<string>("Name") })
                });
                return response;
            });
        }


        [Authorize]
        [Route("product-category")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage GetProductCategory(HttpRequestMessage request, dynamic[] brand)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = null;
                string brandIds = string.Empty;

                if (brand != null && brand.Count() > 0)
                    brandIds = string.Join(",", brand.Select(x => x.brandId).ToArray());

                table = _salesRepository.GetProductCategory(brandIds);
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    productCategories = table.AsEnumerable().Select(x => new { categoryId = x.Field<int>("CategoryId"), categoryName = x.Field<string>("CategoryName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("product")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage GetProduct(HttpRequestMessage request, dynamic[] category)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = null;
                string categoryIds = string.Empty;

                if (category != null && category.Count() > 0)
                    categoryIds = string.Join(",", category.Select(x => x.categoryId).ToArray());

                table = _salesRepository.GetProduct(categoryIds);
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    products = table.AsEnumerable().Select(x => new { productId = x.Field<int>("ProductId"), productName = x.Field<string>("ProductName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("yearly-sales/{year}")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage GetYearlySales(HttpRequestMessage request, int year, dynamic searchCriteria)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                string brandIds = string.Empty;
                string categoryIds = string.Empty;
                string productIds = string.Empty;
                string regionIds = string.Empty;
                string channelIds = string.Empty;
                string showroomIds = string.Empty;
                string reportFilter = string.Empty;

                if (searchCriteria != null)
                {
                    brandIds = searchCriteria.brandIds;
                    categoryIds = searchCriteria.categoryIds;
                    productIds = searchCriteria.productIds;
                    regionIds = searchCriteria.regionIds;
                    channelIds = searchCriteria.channelIds;
                    showroomIds = searchCriteria.showroomIds;
                    reportFilter = searchCriteria.firstReportFilter + "," + searchCriteria.secondReportFilter;
                }

                DataTable table = _salesRepository.GetYearlySales(year, reportFilter, brandIds, categoryIds, productIds, regionIds, channelIds, showroomIds);
                //table = table.DefaultView.ToTable( /*distinct*/ true);
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }

        [Authorize]
        [Route("quarterly-sales/{year}")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage GetQuaterlySales(HttpRequestMessage request, int year, dynamic searchCriteria)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataSet dataSet = null;
                DataTable dtTotalSales = null;
                DataTable dtTotalQty = null;
                DataTable table = new DataTable();
                DataRow row = null;
                int rowCount = 0;
                int index = 0;
                int firstOrdinal = 0;
                int lastOrdinal = 0;
                int count = 0;
                string brandIds = string.Empty;
                string categoryIds = string.Empty;
                string productIds = string.Empty;
                string regionIds = string.Empty;
                string channelIds = string.Empty;
                string showroomIds = string.Empty;
                string reportFilter = string.Empty;
                string salesQuarter = string.Empty;

                if (searchCriteria != null)
                {
                    brandIds = searchCriteria.brandIds;
                    categoryIds = searchCriteria.categoryIds;
                    productIds = searchCriteria.productIds;
                    regionIds = searchCriteria.regionIds;
                    channelIds = searchCriteria.channelIds;
                    showroomIds = searchCriteria.showroomIds;
                    salesQuarter = searchCriteria.salesQuarter;
                    reportFilter = searchCriteria.firstReportFilter + "," + searchCriteria.secondReportFilter;
                }

                dataSet = _salesRepository.GetQuaterlySales(year, salesQuarter, reportFilter, brandIds, categoryIds, productIds, regionIds, channelIds, showroomIds);

                dtTotalSales = dataSet.Tables[0].Copy();
                dtTotalQty = dataSet.Tables[1].Copy();

                foreach (DataColumn item in dataSet.Tables[0].Columns)
                {
                    if (item.ColumnName == "Q1" || item.ColumnName == "Q2" ||
                        item.ColumnName == "Q3" || item.ColumnName == "Q4")
                    {
                        lastOrdinal = dtTotalSales.Columns[item.ColumnName].Ordinal + 1;
                        dtTotalSales.Columns.Add(item.ColumnName + " Quantity", typeof(double)).SetOrdinal(lastOrdinal);

                        if (count == 0)
                            firstOrdinal = dtTotalSales.Columns[item.ColumnName].Ordinal;

                        dtTotalSales.Columns[item.ColumnName].ColumnName = item.ColumnName + " Value";

                        count++;
                    }
                }

                foreach (DataRow item in dtTotalSales.Rows)
                {
                    index = lastOrdinal;

                    for (int i = (dtTotalQty.Columns.Count - 2); i >= firstOrdinal; i--)
                    {
                        row = dtTotalQty.Rows[rowCount];
                        item[index] = !DBNull.Value.Equals(row.ItemArray[i]) ? Convert.ToDouble(row.ItemArray[i]) : 0.0;
                        index = index - 2;
                    }

                    rowCount++;
                }

                foreach (DataColumn item in dtTotalSales.Columns)
                    table.Columns.Add(item.ColumnName);

                foreach (DataRow item in dtTotalSales.Rows)
                {
                    table.Rows.Add(item.ItemArray);
                }

                _unitOfWork.Terminate();

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }

        [Authorize]
        [Route("monthly-sales/{year}")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage GetMonthlySales(HttpRequestMessage request, int year, dynamic searchCriteria)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataSet dataSet = null;
                DataTable dtTotalSales = null;
                DataTable dtTotalQty = null;
                DataTable table = new DataTable();
                DataRow row = null;
                int rowCount = 0;
                int index = 0;
                int firstOrdinal = 0;
                int lastOrdinal = 0;
                int count = 0;
                string brandIds = string.Empty;
                string categoryIds = string.Empty;
                string productIds = string.Empty;
                string regionIds = string.Empty;
                string channelIds = string.Empty;
                string showroomIds = string.Empty;
                string reportFilter = string.Empty;
                string salesMonth = string.Empty;

                if (searchCriteria != null)
                {
                    brandIds = searchCriteria.brandIds;
                    categoryIds = searchCriteria.categoryIds;
                    productIds = searchCriteria.productIds;
                    regionIds = searchCriteria.regionIds;
                    channelIds = searchCriteria.channelIds;
                    showroomIds = searchCriteria.showroomIds;
                    salesMonth = searchCriteria.salesMonth;
                    reportFilter = searchCriteria.firstReportFilter + "," + searchCriteria.secondReportFilter;
                }

                dataSet = _salesRepository.GetMonthlySales(year, salesMonth, reportFilter, brandIds, categoryIds, productIds, regionIds, channelIds, showroomIds);

                dtTotalSales = dataSet.Tables[0].Copy();
                dtTotalQty = dataSet.Tables[1].Copy();

                foreach (DataColumn item in dataSet.Tables[0].Columns)
                {
                    if (item.ColumnName.ToLower() == "jan" || item.ColumnName.ToLower() == "feb" || item.ColumnName.ToLower() == "mar" ||
                        item.ColumnName.ToLower() == "apr" || item.ColumnName.ToLower() == "may" || item.ColumnName.ToLower() == "jun" ||
                        item.ColumnName.ToLower() == "jul" || item.ColumnName.ToLower() == "aug" || item.ColumnName.ToLower() == "sep" ||
                        item.ColumnName.ToLower() == "oct" || item.ColumnName.ToLower() == "nov" || item.ColumnName.ToLower() == "dec")
                    {
                        lastOrdinal = dtTotalSales.Columns[item.ColumnName].Ordinal + 1;
                        dtTotalSales.Columns.Add(item.ColumnName + " Quantity", typeof(double)).SetOrdinal(lastOrdinal);

                        if (count == 0)
                            firstOrdinal = dtTotalSales.Columns[item.ColumnName].Ordinal;

                        dtTotalSales.Columns[item.ColumnName].ColumnName = item.ColumnName + " Value";

                        count++;
                    }
                }

                foreach (DataRow item in dtTotalSales.Rows)
                {
                    index = lastOrdinal;

                    for (int i = (dtTotalQty.Columns.Count - 2); i >= firstOrdinal; i--)
                    {
                        row = dtTotalQty.Rows[rowCount];
                        item[index] = !DBNull.Value.Equals(row.ItemArray[i]) ? Convert.ToDouble(row.ItemArray[i]) : 0.0;
                        index = index - 2;
                    }

                    rowCount++;
                }

                foreach (DataColumn item in dtTotalSales.Columns)
                    table.Columns.Add(item.ColumnName);

                foreach (DataRow item in dtTotalSales.Rows)
                {
                    table.Rows.Add(item.ItemArray);
                }

                _unitOfWork.Terminate();

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }
    }
}
