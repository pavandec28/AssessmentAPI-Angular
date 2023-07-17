using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Generic;

namespace AssessmentAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TFSController : ControllerBase
    {

        private readonly ILogger<TFSController> _logger;
        private readonly IConfiguration _configuration;


        public TFSController(ILogger<TFSController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet(Name = "GetWorkItems")]
        public IEnumerable<WorkItemEntity> Get()
        {
            CommonMethods commonMethods = new CommonMethods(_configuration);
            string projectName = _configuration["AppSettings:projectName"];

            var workItems = commonMethods.PrintOpenBugsAsync(projectName).Result;
            if (workItems != null)
            {
                return commonMethods.GetChildWorkItems(commonMethods.GetSuperParent(workItems).Result).Result.ToArray();
            }
            return null;
        }

        [HttpPost(Name = "UpdateWorkItems")]
        public bool Post(List<WorkItemEntity> workItemEntities)
        {
            CommonMethods commonMethods = new CommonMethods(_configuration);
            bool result = commonMethods.UpdateWorkItem(workItemEntities).Result;
            return result;
        }

    }
}