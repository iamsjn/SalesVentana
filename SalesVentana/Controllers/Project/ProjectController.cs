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
    [RoutePrefix("api/project")]
    public class ProjectController : ApiControllerBase
    {
        IProjectRepository _projectRepository = null;

        public ProjectController(IBaseRepository<Error> errorRepository, IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
            : base(errorRepository, unitOfWork)
        {
            _projectRepository = projectRepository;
        }

        [Authorize]
        [Route("getProject")]
        [HttpGet]
        public HttpResponseMessage GetProject(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = _projectRepository.GetProject();
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    projects = table.AsEnumerable().Select(x => new { projectId = x.Field<int>("ProjectId"), projectName = x.Field<string>("ProjectName") })
                });
                return response;
            });
        }

        [Authorize]
        [Route("project-summary")]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage GetProjectSummary(HttpRequestMessage request, dynamic searchCriteria)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                string projectIds = string.Empty;
                DateTime projectFromDate = new DateTime();
                DateTime projectToDate = new DateTime();

                if (searchCriteria != null)
                {
                    projectIds = searchCriteria.projectIds;
                    projectFromDate = Convert.ToDateTime(searchCriteria.projectFromDate);
                    projectToDate = Convert.ToDateTime(searchCriteria.projectToDate);
                }

                DataTable table = _projectRepository.GetProjectSummary(projectIds, projectFromDate, projectToDate);
                _unitOfWork.Terminate();
                response = request.CreateResponse(HttpStatusCode.OK, new
                {
                    table
                });
                return response;
            });
        }

        [Authorize]
        [Route("project-detail/{projectId}/{breakdown}")]
        [HttpGet]
        public HttpResponseMessage GetProjectDetail(HttpRequestMessage request, int projectId, string breakdown)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                DataTable table = null;
                switch (breakdown.ToLower())
                {
                    case "prquantity":
                        table = _projectRepository.GetPRBreakdown(projectId);
                        break;
                    case "poamount":
                        table = _projectRepository.GetPOBreakdown(projectId);
                        break;
                    case "mrramount":
                        table = _projectRepository.GetMRRBreakdown(projectId);
                        break;
                    case "billamount":
                        table = _projectRepository.GetBillBreakdown(projectId);
                        break;
                    case "otheramount":
                        table = _projectRepository.GetOtherBreakdown(projectId);
                        break;
                    default:
                        break;
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
