using System.Collections.Generic;
namespace project_manage_system_backend.Dtos
{
    public class BoardDto
    {
        public bool success { get; set; }
        public List<BoardDetailDto> data { get; set; }
    }
}
