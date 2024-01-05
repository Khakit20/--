using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_manage_system_backend.Dtos
{
    public class TrelloLoginDto
    {
        public string Name { get; set; }
        public string BoardUrl { get; set; }
        public int ProjectId { get; set; }
    }
}