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

        public SalesController(IBaseRepository<Error> errorRepository, ISalesRepository salesRepository,
            IUnitOfWork unitOfWork)
            : base(errorRepository, unitOfWork)
        {
            _salesRepository = salesRepository;
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
        [Route("initial-data")]
        [HttpGet]
        public HttpResponseMessage GetInitialData(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;

                DataTable brand = _salesRepository.GetBrand();
                DataTable region = _salesRepository.GetRegion();
                DataTable channel = _salesRepository.GetChannel();
                DataTable showroom = _salesRepository.GetShowroom();
                DataTable category = _salesRepository.GetProductCategory(string.Empty);
                DataTable product = _salesRepository.GetProduct(string.Empty);

                _unitOfWork.Terminate();

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    brands = brand.AsEnumerable().Select(x => new { brandId = x.Field<int>("BrandId"), brandName = x.Field<string>("BrandName") }),
                    regions = region.AsEnumerable().Select(x => new { regionId = x.Field<int>("RegionId"), regionName = x.Field<string>("RegionName") }),
                    channels = channel.AsEnumerable().Select(x => new { channelId = x.Field<int>("ChannelId"), channelName = x.Field<string>("ChannelName") }),
                    showrooms = showroom.AsEnumerable().Select(x => new { showroomId = x.Field<int>("ShowroomId"), showroomName = x.Field<string>("Name") }),
                    productCategories = category.AsEnumerable().Select(x => new { categoryId = x.Field<int>("CategoryId"), categoryName = x.Field<string>("CategoryName") }),
                    products = product.AsEnumerable().Select(x => new { productId = x.Field<int>("ProductId"), productName = x.Field<string>("ProductName") })
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
                DataTable dtTotalVal = null;
                DataTable dtTotalQty = new DataTable();
                DataTable dtTemp = null;
                DataRow row = null;
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

                dtTotalVal = _salesRepository.GetYearlySales(year, reportFilter, brandIds, categoryIds, productIds, regionIds, channelIds, showroomIds);
                _unitOfWork.Terminate();

                dtTemp = dtTotalVal.Copy();
                dtTotalVal.Columns.Remove("InvoiceQty");

                foreach (DataColumn item in dtTemp.Columns)
                {
                    if (item.ColumnName.ToLower() != "totalsales(tk)"
                    && item.ColumnName.ToLower() != "retail")
                        dtTotalQty.Columns.Add(item.ColumnName, item.DataType);
                }

                foreach (DataRow item in dtTemp.Rows)
                {
                    row = dtTotalQty.NewRow();
                    foreach (DataColumn col in dtTotalQty.Columns)
                        row[col.ColumnName] = item[col.ColumnName];

                    dtTotalQty.Rows.Add(row);
                }

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    val = dtTotalVal,
                    qty = dtTotalQty
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
                DataTable dtTotalVal = null;
                DataTable dtTotalQty = null;
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

                dtTotalVal = dataSet.Tables[0].Copy();
                dtTotalQty = dataSet.Tables[1].Copy();

                _unitOfWork.Terminate();

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    val = dtTotalVal,
                    qty = dtTotalQty
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
                DataTable dtTotalVal = null;
                DataTable dtTotalQty = null;
                DataTable dtTotalCash = null;
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

                dtTotalVal = dataSet.Tables[0].Copy();
                dtTotalQty = dataSet.Tables[1].Copy();
                dtTotalCash = dataSet.Tables[2].Copy();

                _unitOfWork.Terminate();

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    val = dtTotalVal,
                    qty = dtTotalQty,
                    cash = dtTotalCash
                });
                return response;
            });
        }
    }
}
