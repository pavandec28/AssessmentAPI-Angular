namespace AssessmentAPI
{
    public class WorkItemEntity
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string EpicLevel { get; set; }
        public string IssueLevel { get; set; }
        public string TaskLevel { get; set; }
    }
}