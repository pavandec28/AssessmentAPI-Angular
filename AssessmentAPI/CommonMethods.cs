using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;

namespace AssessmentAPI
{
    public class CommonMethods
    {
        private readonly Uri uri;
        private readonly string personalAccessToken;
        private readonly string orgName;
        private static WorkItemTrackingHttpClient workItemClient = null;
        private readonly IConfiguration _configuration;

        #region Constructor
        public CommonMethods(IConfiguration configuration)
        {
            _configuration = configuration;
            this.uri = new Uri("https://dev.azure.com/" + _configuration["AppSettings:orgName"]);
            this.personalAccessToken = _configuration["AppSettings:personalAccessToken"];
            CreateWorkItemTrackingHttpClient();
        }
        #endregion

        #region Public methods for Work Item API
        /// <summary>
        ///     Execute a WIQL (Work Item Query Language) query to return a list of open bugs.
        /// </summary>
        /// <param name="project">The name of your project within your organization.</param>
        /// <returns>A list of <see cref="WorkItemEntity"/> objects representing all the open bugs.</returns>
        public async Task<IList<WorkItem>> QueryOpenBugs(string project)
        {
            try
            {
                // create a wiql object and build our query
                var wiql = new Wiql()
                {
                    // NOTE: Even if other columns are specified, only the ID & URL are available in the WorkItemReference
                    Query = "Select [Id], [Title]" +
                            "From WorkItems "
                };

                // create instance of work item tracking http client
                if (workItemClient != null)
                {
                    // execute the query to get the list of work items in the results
                    var result = await workItemClient.QueryByWiqlAsync(wiql).ConfigureAwait(false);
                    var ids = result.WorkItems.Select(item => item.Id).ToArray();

                    // some error handling
                    if (ids.Length == 0)
                    {
                        return Array.Empty<WorkItem>();
                    }

                    // build a list of the fields we want to see
                    var fields = new[] { "System.Id", "System.Title", "System.State" };

                    // get work items for the ids found in query
                    return await workItemClient.GetWorkItemsAsync(ids, fields, result.AsOf).ConfigureAwait(false);
                }
                return Array.Empty<WorkItem>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Execute a WIQL (Work Item Query Language) query to print a list of open bugs.
        /// </summary>
        /// <param name="project">The name of your project within your organization.</param>
        /// <returns>An async task.</returns>
        public async Task<List<WorkItemEntity>> PrintOpenBugsAsync(string project)
        {
            try
            {
                var workItems = await this.QueryOpenBugs(project).ConfigureAwait(false);

                Console.WriteLine("Query Results: {0} items found", workItems.Count);

                var lstWorkItems = new List<WorkItemEntity>();
                // loop though work items and write to console
                foreach (var workItem in workItems)
                {
                    lstWorkItems.Add(new WorkItemEntity()
                    {
                        Id = workItem.Id,
                        Title = Convert.ToString(workItem.Fields["System.Title"])
                    });
                }
                return lstWorkItems;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<WorkItemEntity>> GetChildWorkItems(List<WorkItemEntity> workItemEntities)
        {
            try
            {
                foreach (var workItem in workItemEntities)
                {
                    WorkItem tempWorkItem = await workItemClient.GetWorkItemAsync(workItem.Id.Value, expand: WorkItemExpand.Relations);
                    var childRelations = tempWorkItem.Relations.Where(r => r.Rel == "System.LinkTypes.Hierarchy-Forward");
                    foreach (var relation in childRelations)
                    {
                        // Extract the child work item ID from the relation URL
                        int childId = int.Parse(relation.Url.Split('/').Last());

                        // Get the child work item
                        if (!string.IsNullOrEmpty(workItem.EpicLevel) && string.IsNullOrEmpty(workItem.IssueLevel))
                            workItemEntities.Where(x => x.Id == childId).FirstOrDefault().IssueLevel = "Issue";
                        else if (string.IsNullOrEmpty(workItem.EpicLevel) && !string.IsNullOrEmpty(workItem.IssueLevel))
                            workItemEntities.Where(x => x.Id == childId).FirstOrDefault().TaskLevel = "Task";

                    }

                }
                return workItemEntities;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<WorkItemEntity>> GetSuperParent(List<WorkItemEntity> workItemEntities)
        {
            try
            {
                int superParent = 0;
                foreach (var workItem in workItemEntities)
                {
                    // Get the work item
                    WorkItem tempWorkItem = await workItemClient.GetWorkItemAsync(workItem.Id.Value, expand: WorkItemExpand.Relations);

                    // Get the parent relations for the work item
                    var parentRelations = tempWorkItem.Relations.Where(r => r.Rel == "System.LinkTypes.Hierarchy-Reverse");
                    if (parentRelations.Count() == 0)
                    {
                        workItemEntities.Where(x => x.Id == workItem.Id).FirstOrDefault().EpicLevel = "Epic";
                    }

                }
                return workItemEntities;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateWorkItem(List<WorkItemEntity> workItemEntities)
        {
            try
            {
                foreach (var workItem in workItemEntities)
                {
                    // Get the work item
                    WorkItem tempWorkItem = await workItemClient.GetWorkItemAsync(workItem.Id.Value);
                    // Update the title
                    JsonPatchDocument patchDocument = new JsonPatchDocument();
                    patchDocument.Add(new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.Title",
                        Value = workItem.Title
                    });

                    WorkItem updatedWorkItem = await workItemClient.UpdateWorkItemAsync(patchDocument, workItem.Id.Value);

                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Private Methods
        private void CreateWorkItemTrackingHttpClient()
        {
            try
            {
                var credentials = new VssBasicCredential(string.Empty, this.personalAccessToken);
                workItemClient = new WorkItemTrackingHttpClient(this.uri, credentials);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
