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
    [RoutePrefix("api/lettercredit")]
    public class LetterCreditController : ApiControllerBase
    {
        ILetterCreditRepository _letterCreditRepository = null;
        IUnitOfWork _unitOfWork = null;

        public LetterCreditController(IBaseRepository<Error> errorRepository, ILetterCreditRepository letterCreditRepository,
            IUnitOfWork unitOfWork)
            : base(errorRepository, unitOfWork)
        {
            _letterCreditRepository = letterCreditRepository;
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [Route("status")]
        [HttpGet]
        public HttpResponseMessage GetStatus(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _letterCreditRepository.GetStatus();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    statuses = table.AsEnumerable().Select(x => new { statusId = x.Field<int>("StatusId"), statusName = x.Field<string>("StatusName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("bank")]
        [HttpGet]
        public HttpResponseMessage GetBank(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _letterCreditRepository.GetBank();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    banks = table.AsEnumerable().Select(x => new { bankId = x.Field<int>("BankId"), bankName = x.Field<string>("BankName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("supplier")]
        [HttpGet]
        public HttpResponseMessage GetSupplier(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _letterCreditRepository.GetSupplier();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    suppliers = table.AsEnumerable().Select(x => new { supplierId = x.Field<int>("SupplierId"), supplierName = x.Field<string>("SupplierName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("term")]
        [HttpGet]
        public HttpResponseMessage GetTerm(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _letterCreditRepository.GetTerm();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    terms = table.AsEnumerable().Select(x => new { termId = x.Field<int>("TermId"), termName = x.Field<string>("TermName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("lc-summary")]
        [HttpGet]
        public HttpResponseMessage GetLCSummary(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _letterCreditRepository.GetLCSummary();
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
