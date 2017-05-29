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

        public LetterCreditController(IBaseRepository<Error> errorRepository, ILetterCreditRepository letterCreditRepository,
            IUnitOfWork unitOfWork)
            : base(errorRepository, unitOfWork)
        {
            _letterCreditRepository = letterCreditRepository;
        }

        [Authorize]
        [Route("lcnos")]
        [HttpGet]
        public HttpResponseMessage GetLCNo(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _letterCreditRepository.GetLCNo();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    lcNos = table.AsEnumerable().Select(x => new { lcId = x.Field<int>("LCId"), lcNo = x.Field<string>("LCNo") })
                });
                return response;
            });
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
        [Route("initial-data")]
        [HttpGet]
        public HttpResponseMessage GetInitialData(HttpRequestMessage request)
        {

            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable lcNos = _letterCreditRepository.GetLCNo();
                DataTable status = _letterCreditRepository.GetStatus();
                DataTable bank = _letterCreditRepository.GetBank();
                DataTable supplier = _letterCreditRepository.GetSupplier();
                DataTable term = _letterCreditRepository.GetTerm();
                _unitOfWork.Terminate();

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    lcNos = lcNos.AsEnumerable().Select(x => new { lcId = x.Field<int>("LCId"), lcNo = x.Field<string>("LCNo") }),
                    statuses = status.AsEnumerable().Select(x => new { statusId = x.Field<int>("StatusId"), statusName = x.Field<string>("StatusName") }),
                    banks = bank.AsEnumerable().Select(x => new { bankId = x.Field<int>("BankId"), bankName = x.Field<string>("BankName") }),
                    suppliers = supplier.AsEnumerable().Select(x => new { supplierId = x.Field<int>("SupplierId"), supplierName = x.Field<string>("SupplierName") }),
                    terms = term.AsEnumerable().Select(x => new { termId = x.Field<int>("TermId"), termName = x.Field<string>("TermName") })
                });
                return response;
            });
        }


        [Authorize]
        [Route("lc-summary")]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage GetLCSummary(HttpRequestMessage request, dynamic searchCriteria)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                string lcIds = string.Empty;
                string statusIds = string.Empty;
                string supplierIds = string.Empty;
                string bankIds = string.Empty;
                string termIds = string.Empty;
                DateTime issueFromDate = new DateTime();
                DateTime issueToDate = new DateTime();

                if (searchCriteria != null)
                {
                    lcIds = searchCriteria.lcIds;
                    statusIds = searchCriteria.statusIds;
                    supplierIds = searchCriteria.supplierIds;
                    bankIds = searchCriteria.bankIds;
                    termIds = searchCriteria.termIds;
                    issueFromDate = Convert.ToDateTime(searchCriteria.issueFromDate);
                    issueToDate = Convert.ToDateTime(searchCriteria.issueToDate);
                }

                DataTable table = _letterCreditRepository.GetLCSummary(lcIds, statusIds, supplierIds, bankIds, termIds, issueFromDate,  issueToDate);
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }

        [Authorize]
        [Route("lc-items/{id}")]
        [HttpGet]
        public HttpResponseMessage GetLCItems(HttpRequestMessage request, int id)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _letterCreditRepository.GetLCItems(id);
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }

        [Authorize]
        [Route("lc-expenditures/{id}")]
        [HttpGet]
        public HttpResponseMessage GetLCExpenditures(HttpRequestMessage request, int id)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _letterCreditRepository.GetLCExpenditures(id);
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }

        [Authorize]
        [Route("lc-activities/{id}")]
        [HttpGet]
        public HttpResponseMessage GetLCActivities(HttpRequestMessage request, int id)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _letterCreditRepository.GetLCActivities(id);
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }

        [Authorize]
        [Route("lc-detail/{id}")]
        [HttpGet]
        public HttpResponseMessage GetLCDetail(HttpRequestMessage request, int id)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable lcItem = _letterCreditRepository.GetLCItems(id);
                DataTable expenditure = _letterCreditRepository.GetLCExpenditures(id);
                DataTable activity = _letterCreditRepository.GetLCActivities(id);
                _unitOfWork.Terminate();

                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    lcItem, expenditure, activity
                });
                return response;
            });
        }
    }
}
