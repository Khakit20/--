using project_manage_system_backend.Dtos;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using project_manage_system_backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using project_manage_system_backend.Services;
using project_manage_system_backend.Models;
using RichardSzalay.MockHttp;
using System.Net;

namespace PMS_test.ControllersTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class JiraControllerTests : BaseControllerTests
    {
        private readonly JiraController _jiraController;
        private readonly new HttpClient _client;
        private readonly JiraService _jiraService;


        JiraLoginDto jiraLoginDto = new JiraLoginDto{
            DomainURL = "DomainURL",
            APIToken = "APIToken",
            Account = "Account",
            BoardId = 1,
            ProjectId = 1
        };

        JiraLoginDto dtoForFail = new JiraLoginDto{
            DomainURL = "failDomainURL",
            APIToken = "APIToken",
            Account = "Account",
            BoardId = 1,
            ProjectId = 1
        };

        private Project _project1;  

        public JiraControllerTests() : base()
        {

            _dbContext.Database.EnsureCreated();
            InitialDatabase();
            _client = CreateMockClient();
            _jiraService = new JiraService(_dbContext, _client);
            _jiraController = new JiraController(_dbContext);
            _jiraController.setJiraService(_jiraService);

        }

        private void InitialDatabase()
        {
            _project1 = new Project
            {
                ID = 1,
                Name = "Test",
                Owner = null,
                Users = null
            };
            _dbContext.Projects.Add(_project1);
            _dbContext.SaveChanges();
        }

        private HttpClient CreateMockClient()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Get, "https://DomainURL.atlassian.net/rest/agile/1.0/board")
                .Respond("application/json", getResponseOfBoardDetail());
            mockHttp.Fallback.WithAny().Respond(HttpStatusCode.BadRequest);
            

            return mockHttp.ToHttpClient();
        }

        private string getResponseOfBoardDetail()
        {
            string responseData = @"{
                'maxResults': 50,
                'startAt': 0,
                'total': 2,
                'isLast': true,
                'values': [
                        {
                        'id': 1,
                        'self': 'fakeURL',
                        'name': 'PD board',
                        'type': 'simple',
                        'location': {
                            'projectId': 10000,
                            'displayName': 'PD (PD)',
                            'projectName': 'PD',
                            'projectKey': 'PD',
                            'projectTypeKey': 'software',
                            'avatarURI': 'fakeAvatarURI',
                            'name': 'PD (PD)'
                        }
                    },
                    {
                        'id': 2,
                        'self': 'fakeURL2',
                        'name': 'TFJA board',
                        'type': 'simple',
                        'location': {
                            'projectId': 10001,
                            'displayName': 'test_for_jira_api (TFJA)',
                            'projectName': 'test_for_jira_api',
                            'projectKey': 'TFJA',
                            'projectTypeKey': 'software',
                            'avatarURI': 'fakeAvatarURI2',
                            'name': 'test_for_jira_api (TFJA)'
                        }
                    }
                ]
            }";

            return responseData;
        }
        
        [Fact]
        public void TestCreateJiraRepoSuccess() {
            IActionResult iActionResult = _jiraController.CreateJiraRepo(jiraLoginDto);
            var okResult = iActionResult as OkObjectResult;
            var responseDto = okResult.Value as ResponseDto;

            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Added Success", responseDto.message);
            Assert.True(responseDto.success);
        }

        [Fact]
        public void TestCreateJiraRepoFail() {
            IActionResult iActionResult = _jiraController.CreateJiraRepo(dtoForFail);
            var okResult = iActionResult as OkObjectResult;
            var responseDto = okResult.Value as ResponseDto;

            Assert.Equal(200, okResult.StatusCode);
            Assert.False(responseDto.success);
            Assert.True(responseDto.message.Contains("Added Error: One or more errors occurred."));
        }
        
        [Fact]
        public void TestGetBoardInfo() {
            Task<IActionResult> iActionResult = _jiraController.GetBoardInfo(jiraLoginDto);
            var okResult = iActionResult.Result as OkObjectResult;
            Assert.Equal(200, okResult.StatusCode);
        }
    }

}