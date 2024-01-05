using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;

namespace project_manage_system_backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class JiraController : ControllerBase
    {
        private JiraService _jiraService;

        public JiraController(PMSContext dbContext)
        {
            _jiraService = new JiraService(dbContext);
        }

        public void setJiraService(JiraService jiraService)
        {
            _jiraService = jiraService;
        }

        [Authorize]
        [HttpPost("createRepo")]
        public IActionResult CreateJiraRepo(JiraLoginDto jiraLoginDto)
        {
            try
            {
                _jiraService.CreateJiraRepo(jiraLoginDto);
                return Ok(new ResponseDto
                {
                    success = true,
                    message = "Added Success"
                });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseDto
                {
                    success = false,
                    message = "Added Error: " + ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost("boardInfo")]
        public async Task<IActionResult> GetBoardInfo(JiraLoginDto jiraLoginDto)
        {
            try
            {
                return Ok(new BoardDto
                {
                    success = true,
                    data = _jiraService.getBoardDetail(jiraLoginDto).Result
                });
            }
            catch (Exception ex)
            {
                return Ok(new BoardDto
                {
                    success = false,
                    data = null
                });
            }
        }

        [Authorize]
        [HttpPost("issue")]
        public async Task<IActionResult> GetAllIssueByBoardId(RepoIdDto repoIdDto)
        {
            return Ok(_jiraService.getAllIssueByBoardId(repoIdDto));
        }

        [Authorize]
        [HttpPost("issueInfo")]
        public async Task<IActionResult> GetIssueBySprintId(IdDto IdDto)
        {
            return Ok(_jiraService.GetIssueBySprintId(IdDto));
        }

        [Authorize]
        [HttpPost("sprint")]
        public async Task<IActionResult> GetAllSprintByBoardId(IdDto IdDto)
        {
            return Ok(_jiraService.GetAllSprintByBoardId(IdDto));
        }
    }
}
