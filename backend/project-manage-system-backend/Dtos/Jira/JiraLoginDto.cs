namespace project_manage_system_backend.Dtos
{
    public class JiraLoginDto
    {
        public string DomainURL { get; set; }
        public string APIToken { get; set; }
        public string Account { get; set; }
        public int BoardId { get; set; }
        public int ProjectId { get; set; }
    }
}
