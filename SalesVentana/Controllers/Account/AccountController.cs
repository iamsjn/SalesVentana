using SalesVentana.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SalesVentana.Data;
using SalesVentana.BO;

namespace SalesVentana.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/account")]
    public class AccountController : ApiControllerBase
    {
        private readonly IMembershipService _membershipService;

        public AccountController(IMembershipService membershipService, IBaseRepository<Error> errorRepository, IUnitOfWork unitOfWork) :
            base(errorRepository, unitOfWork)
        {
            _membershipService = membershipService;
        }

        [AllowAnonymous]
        [Route("authenticate")]
        [HttpPost]
        public HttpResponseMessage Login(HttpRequestMessage request, LoginViewModel user)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                if (ModelState.IsValid)
                {
                    MembershipContext userContext = _membershipService.ValidateUser(user.Username, user.Password);
                    if (userContext != null && userContext.User != null)
                        response = request.CreateResponse(HttpStatusCode.OK, new { success = true });
                    else
                        response = request.CreateResponse(HttpStatusCode.OK, new { success = false });
                }
                else
                {
                    response = request.CreateResponse(HttpStatusCode.OK, new { success = false });
                }

                return response;
            });
        }

        [AllowAnonymous]
        [Route("register")]
        [HttpPost]
        public HttpResponseMessage Register(HttpRequestMessage request, RegistrationViewModel user)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                if (!ModelState.IsValid)
                {
                    response = request.CreateResponse(HttpStatusCode.BadRequest, new { success = false });
                }
                else
                {
                    BO.User _user = _membershipService.CreateUser(user.Username, user.Email, user.Password, new int[] { 1 });
                    if (_user != null)
                        response = request.CreateResponse(HttpStatusCode.OK, new { success = true });
                    else
                        response = request.CreateResponse(HttpStatusCode.OK, new { success = false });
                }
                return response;
            });
        }
    }
}
