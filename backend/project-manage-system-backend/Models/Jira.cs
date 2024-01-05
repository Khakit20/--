using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace project_manage_system_backend.Models
{
    public class Jira
    {
        public string DomainURL { get; set; }
        public string APIToken { get; set; }
        public string Account { get; set; }
        public int BoardId { get; set; }
        [Key]
        public int Id { get; set; }
    }
}
