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
    [RoutePrefix("api/receivable-sales")]
    public class ReceivableSalesController : ApiControllerBase
    {
        IReceivableSalesRepository _receivableSalesRepository = null;

        public ReceivableSalesController(IBaseRepository<Error> errorRepository, IReceivableSalesRepository receivableSalesRepository,
            IUnitOfWork unitOfWork)
            : base(errorRepository, unitOfWork)
        {
            _receivableSalesRepository = receivableSalesRepository;
        }

        [Authorize]
        [Route("channel")]
        [HttpGet]
        public HttpResponseMessage GetChannel(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _receivableSalesRepository.GetChannel();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    channels = table.AsEnumerable().Select(x => new { channelId = x.Field<int>("channelId"), channelName = x.Field<string>("ChannelName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("workorder")]
        [HttpGet]
        public HttpResponseMessage GetWorkorder(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _receivableSalesRepository.GetWorkorder();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    workorders = table.AsEnumerable().Select(x => new { workorderId = x.Field<int>("WorkorderId"), workorderName = x.Field<string>("WorkorderName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("customer")]
        [HttpGet]
        public HttpResponseMessage GetCustomer(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _receivableSalesRepository.GetCustomer();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    customers = table.AsEnumerable().Select(x => new { customerId = x.Field<int>("CustomerId"), customerName = x.Field<string>("CustomerName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("sales-person")]
        [HttpGet]
        public HttpResponseMessage GetSalesPerson(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _receivableSalesRepository.GetSalesPerson();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    salesPersons = table.AsEnumerable().Select(x => new { salesPersonId = x.Field<int>("SalesPersonId"), salesPersonName = x.Field<string>("SalesPersonName") })
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
                DataTable channels = _receivableSalesRepository.GetChannel();
                DataTable workorders = _receivableSalesRepository.GetWorkorder();
                DataTable customers = _receivableSalesRepository.GetCustomer();
                DataTable salesPersons = _receivableSalesRepository.GetSalesPerson();
                _unitOfWork.Terminate();

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    channels = channels.AsEnumerable().Select(x => new { channelId = x.Field<int>("channelId"), channelName = x.Field<string>("ChannelName") }),
                    workorders = workorders.AsEnumerable().Select(x => new { workorderId = x.Field<int>("WorkorderId"), workorderName = x.Field<string>("WorkorderName") }),
                    customers = customers.AsEnumerable().Select(x => new { customerId = x.Field<int>("CustomerId"), customerName = x.Field<string>("CustomerName") }),
                    salesPersons = salesPersons.AsEnumerable().Select(x => new { salesPersonId = x.Field<int>("SalesPersonId"), salesPersonName = x.Field<string>("SalesPersonName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("rs-summary")]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage GetRSSummary(HttpRequestMessage request, dynamic searchCriteria)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                string channelIds = string.Empty;
                string workorderIds = string.Empty;
                string customerIds = string.Empty;
                string salesPersonIds = string.Empty;

                if (searchCriteria != null)
                {
                    channelIds = searchCriteria.channelIds;
                    workorderIds = searchCriteria.workorderIds;
                    customerIds = searchCriteria.customerIds;
                    salesPersonIds = searchCriteria.salesPersonIds;
                }

                DataTable table = _receivableSalesRepository.GetRSSummary(channelIds, workorderIds, customerIds, salesPersonIds);
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }

        [Authorize]
        [Route("rs-detail/{id}")]
        [HttpGet]
        public HttpResponseMessage GetRSDetail(HttpRequestMessage request, int id)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _receivableSalesRepository.GetRSDetail(id);
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
