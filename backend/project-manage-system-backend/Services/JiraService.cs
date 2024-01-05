using project_manage_system_backend.Shares;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace project_manage_system_backend.Services
{
    public class JiraService : BaseService
    {
        private readonly HttpClient _httpClient;
        public JiraService(PMSContext dbContext, HttpClient client = null) : base(dbContext) {
            _httpClient = client ?? new HttpClient();
        }

        public void CreateJiraRepo(JiraLoginDto dto)
        {
            List<BoardDetailDto> boardDetailDtos = getBoardDetail(dto).Result;
            string boardName = "";
            foreach(BoardDetailDto boardDetailDto in boardDetailDtos) {
                if (boardDetailDto.id == dto.BoardId) {
                    boardName = boardDetailDto.name;
                }
            }
            var project = _dbContext.Projects.Where(p => p.ID == dto.ProjectId).First();
            Repo repo = new Repo
            {
                Name = boardName, // TODO: IsSonarqube要拿掉
                IsSonarqube = false,
                Project = project,
                Type = "Jira"
            };
            _dbContext.Repositories.Add(repo);
            if (_dbContext.SaveChanges() == 0)
            {
                throw new Exception("create repository fail");
            }
            repo = _dbContext.Repositories.Where(repo => repo.Name == boardName).OrderByDescending(repo => repo.ID).First();

            Jira jira = new Jira
            {
                DomainURL = dto.DomainURL,
                APIToken = dto.APIToken,
                Account = dto.Account,
                BoardId = dto.BoardId,
                Id = repo.ID
            };
            _dbContext.Jiras.Add(jira);
            if (_dbContext.SaveChanges() == 0)
            {
                throw new Exception("create jira fail");
            }
        }

        public virtual async Task<List<BoardDetailDto>> getBoardDetail(JiraLoginDto jiraLoginDto){
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{jiraLoginDto.Account}:{jiraLoginDto.APIToken}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(byteArray));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await _httpClient.GetAsync($"https://{jiraLoginDto.DomainURL}.atlassian.net/rest/agile/1.0/board");
            if(!response.IsSuccessStatusCode) {
                throw new Exception("get board detail fail");
            }
            string boardInfo = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(boardInfo);
            List<BoardDetailDto> boardDetailDtos = new List<BoardDetailDto>();
            foreach(JObject board in json.GetValue("values"))
            {
                boardDetailDtos.Add(new BoardDetailDto{
                    id = (int)board.GetValue("id"),
                    name = (string)board.GetValue("name")
                });
            }
            return boardDetailDtos;
        }

        public virtual async Task<List<JiraIssueDto>> getAllIssueByBoardId(RepoIdDto repoIdDto){
            try{
                var jira = _dbContext.Jiras.Where(p => p.Id == repoIdDto.RepoId).First();
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{jira.Account}:{jira.APIToken}");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(byteArray));
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await _httpClient.GetAsync($"https://{jira.DomainURL}.atlassian.net/rest/agile/1.0/board/{jira.BoardId}/issue");
                if(!response.IsSuccessStatusCode) {
                    throw new Exception("get board detail fail");
                }
                string issueInfo = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(issueInfo);
                List<JiraIssueDto> jiraIssueDtos = new List<JiraIssueDto>();
                foreach(JObject issue in json.GetValue("issues"))
                {
                    JObject fields = (JObject)issue.GetValue("fields");
                    jiraIssueDtos.Add(new JiraIssueDto{
                        Key = (string)issue.GetValue("key"),
                        Summary = (string)fields.GetValue("summary"),
                        Type = (string)((JObject)fields.GetValue("issuetype")).GetValue("name"),
                        Status = (string)((JObject)fields.GetValue("status")).GetValue("name"),
                        Priority = (string)((JObject)fields.GetValue("priority")).GetValue("name"),
                        Resolution = fields.GetValue("resolution").ToString() == "" ? "Unresolved" : "Done",
                        Created = (string)fields.GetValue("created"),
                        Updated = (string)fields.GetValue("updated"),
                        Label = ((JArray)fields.GetValue("labels")).ToObject<List<string>>()
                    });
                }
                return jiraIssueDtos;
            }catch(Exception ex){
                return new List<JiraIssueDto>();
            }
        }

        public virtual async Task<List<JiraIssueDto>> GetIssueBySprintId(IdDto idDto){
            try{
                var jira = _dbContext.Jiras.Where(p => p.Id == idDto.RepoId).First();
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{jira.Account}:{jira.APIToken}");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(byteArray));
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await _httpClient.GetAsync($"https://{jira.DomainURL}.atlassian.net/rest/agile/1.0/sprint/{idDto.SprintId}/issue");
                if(!response.IsSuccessStatusCode) {
                    throw new Exception("get sprint detail fail");
                }
                string issueInfo = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(issueInfo);
                List<JiraIssueDto> jiraIssueDtos = new List<JiraIssueDto>();
                foreach(JObject issue in json.GetValue("issues"))
                {
                    JObject fields = (JObject)issue.GetValue("fields");
                    List<JiraIssueDto> subtasks = new List<JiraIssueDto>();
                    foreach(JObject subtask in fields.GetValue("subtasks")){
                        JObject subTaskFields = (JObject)subtask.GetValue("fields");
                        subtasks.Add(new JiraIssueDto{
                            Key = (string)subtask.GetValue("key"),
                            Summary = (string)subTaskFields.GetValue("summary"),
                            Status = (string)((JObject)subTaskFields.GetValue("status")).GetValue("name")
                        });
                    }
                    jiraIssueDtos.Add(new JiraIssueDto{
                        Key = (string)issue.GetValue("key"),
                        Summary = (string)fields.GetValue("summary"),
                        Status = (string)((JObject)fields.GetValue("status")).GetValue("name"),
                        estimatePoint = (int)fields.GetValue("customfield_10016"), //TODO:CUSTOMFIELD 不要用寫死的
                        Label = ((JArray)fields.GetValue("labels")).ToObject<List<string>>(),
                        Priority = (string)((JObject)fields.GetValue("priority")).GetValue("name"),
                        Description = (string)fields.GetValue("description"),
                        SubTasks = subtasks
                    });
                }
                return jiraIssueDtos;
            }catch(Exception ex){
                return new List<JiraIssueDto>();
            }
        }

        public virtual async Task<List<SprintDto>> GetAllSprintByBoardId(IdDto idDto){
            try{
                var jira = _dbContext.Jiras.Where(p => p.Id == idDto.RepoId).First();
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{jira.Account}:{jira.APIToken}");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(byteArray));
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await _httpClient.GetAsync($"https://{jira.DomainURL}.atlassian.net/rest/agile/1.0/board/{jira.BoardId}/sprint");
                if(!response.IsSuccessStatusCode) {
                    throw new Exception("get sprint fail");
                }
                string issueInfo = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(issueInfo);
                List<SprintDto> SprintDtos = new List<SprintDto>();
                foreach(JObject value in json.GetValue("values"))
                {
                    SprintDtos.Add(new SprintDto{
                        Id = (int)value.GetValue("id"),
                        Name = (string)value.GetValue("name"),
                        Goal = (string)value.GetValue("goal")
                    });
                }
                return SprintDtos;
            }catch(Exception ex){
                return new List<SprintDto>();
            }
        }
    }
}