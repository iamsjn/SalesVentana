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
        ISalesRepository _productCategoryWiseSalesRepository = null;
        public SalesController(IBaseRepository<Error> errorRepository, ISalesRepository productCategoryWiseSalesRepository,
            IUnitOfWork unitOfWork)
            : base(errorRepository, unitOfWork)
        {
            _productCategoryWiseSalesRepository = productCategoryWiseSalesRepository;
        }

        [Authorize]
        [Route("brand")]
        [HttpGet]
        public HttpResponseMessage GetBrand(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _productCategoryWiseSalesRepository.GetBrand();

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
                DataTable table = _productCategoryWiseSalesRepository.GetRegion();

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
                DataTable table = _productCategoryWiseSalesRepository.GetChannel();

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    channels = table.AsEnumerable().Select(x => new { channelId = x.Field<int>("ChannelId"), channelName = x.Field<string>("ChannelName") })
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

                table = _productCategoryWiseSalesRepository.GetProductCategory(brandIds);

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

                table = _productCategoryWiseSalesRepository.GetProduct(categoryIds);

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
                string reportType = string.Empty;

                if (searchCriteria != null)
                {
                    brandIds = searchCriteria.brandIds;
                    categoryIds = searchCriteria.categoryIds;
                    productIds = searchCriteria.productIds;
                    regionIds = searchCriteria.regionIds;
                    channelIds = searchCriteria.channelIds;
                    reportType = "brandType:" + searchCriteria.brandType + "," + "categoryType:" + searchCriteria.categoryType + "," + "productType:" + searchCriteria.productType;
                }

                DataTable table = _productCategoryWiseSalesRepository.GetYearlySales(year, reportType, brandIds, categoryIds, productIds, regionIds, channelIds);
                table = table.DefaultView.ToTable( /*distinct*/ true);
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }
    }
}
