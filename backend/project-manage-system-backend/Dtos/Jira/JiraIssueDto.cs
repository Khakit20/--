using System.Collections.Generic;

namespace project_manage_system_backend.Dtos
{
    public class JiraIssueDto
    {
        public string Summary { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Key { get; set; }
        public string Resolution { get; set; }
        public string Created { get; set; }
        public string Updated { get; set; }
        public List<string> Label { get; set; }
        public int estimatePoint { get; set; }
        public string Description { get; set; }
        public List<JiraIssueDto> SubTasks { get; set; }
    }
}