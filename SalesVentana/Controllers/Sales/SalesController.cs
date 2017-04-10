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
                    productCategories = table.AsEnumerable().Select(x => new { categoryId = x.Field<int>("CategoryId"), name = x.Field<string>("CategoryName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("product")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage GetProduct(HttpRequestMessage request, dynamic[] productCategory)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = null;
                string productIds = string.Empty;

                if (productCategory != null && productCategory.Count() > 0)
                    productIds = string.Join(",", productCategory.Select(x => x.categoryId).ToArray());

                table = _productCategoryWiseSalesRepository.GetProduct(productIds);

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
        public HttpResponseMessage GetYearlySales(HttpRequestMessage request, int year)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _productCategoryWiseSalesRepository.GetYearlySales(year);

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }
    }
}
