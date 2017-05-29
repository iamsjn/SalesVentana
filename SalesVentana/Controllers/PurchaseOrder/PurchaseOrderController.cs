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
    [RoutePrefix("api/po")]
    public class PurchaseOrderController : ApiControllerBase
    {
        IPurchaseOrderRepository _purchaseOrderRepository = null;

        public PurchaseOrderController(IBaseRepository<Error> errorRepository, IPurchaseOrderRepository purchaseOrderRepository,
            IUnitOfWork unitOfWork)
            : base(errorRepository, unitOfWork)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        [Authorize]
        [Route("po-detail")]
        [HttpGet]
        public HttpResponseMessage GetPurchaseOrderDetail(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;

                DataSet ds = _purchaseOrderRepository.GetPurchaseOrderDetail();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    bp = ds.Tables[0],
                    item = ds.Tables[1]
                });
                return response;
            });
        }
    }
}
