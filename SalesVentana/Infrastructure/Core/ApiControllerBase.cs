using SalesVentana.BO;
using SalesVentana.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SalesVentana
{
    public class ApiControllerBase : ApiController
    {
        protected readonly IBaseRepository<Error> _errorsRepository;
        protected readonly IUnitOfWork _unitOfWork;

        public ApiControllerBase(IBaseRepository<Error> errorsRepository, IUnitOfWork unitOfWork)
        {
            _errorsRepository = errorsRepository;
            _unitOfWork = unitOfWork;
        }

        protected HttpResponseMessage CreateHttpResponse(HttpRequestMessage request, Func<HttpResponseMessage> function)
        {
            HttpResponseMessage response = null;
            try
            {
                response = function.Invoke();
            }
            catch (Exception ex)
            {
                LogError(ex);
                response = request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            return response;
        }

        private void LogError(Exception ex)
        {
            try
            {
                //Error _error = new Error()
                //{
                //    Message = ex.Message,
                //    StackTrace = ex.StackTrace,
                //    DateCreated = DateTime.Now
                //};
                //_errorsRepository.Add(_error);
                //_unitOfWork.Commit();
            }
            catch { }
        }
    }
}