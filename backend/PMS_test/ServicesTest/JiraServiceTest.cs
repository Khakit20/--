using Moq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using project_manage_system_backend.Models;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Net;
using System.Data.Common;
using System.Net.Http;
using System;
using Xunit;


namespace PMS_test.ServicesTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class JiraServiceTest
    {
        private readonly PMSContext _dbContext;
        private readonly HttpClient _client;
        private readonly JiraService _jiraService;
        public Mock<JiraService> mockJiraService = new Mock<JiraService>();

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
        JiraLoginDto jiraLoginDtoNoData = new JiraLoginDto{
            DomainURL = "",
            APIToken = "",
            Account = "",
            BoardId = 1,
            ProjectId = 1
        };

        JiraLoginDto jiroLoginDtoNull = new JiraLoginDto();

        List<BoardDetailDto> boardDetailDtos = new List<BoardDetailDto>();      
        private Project _project1;  

        public JiraServiceTest()
        {
            _dbContext = new PMSContext(new DbContextOptionsBuilder<PMSContext>()
               .UseSqlite(CreateInMemoryDatabase()).Options);
            _dbContext.Database.EnsureCreated();
            InitialDatabase();
            _client = CreateMockClient();
            _jiraService = new JiraService(_dbContext, _client);
            boardDetailDtos.Add(new BoardDetailDto{
                id = 1,
                name = "Test1"
            });
            boardDetailDtos.Add(new BoardDetailDto{
                id = 2,
                name = "Test1"
            });
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

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            return connection;
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

        private HttpResponseMessage getResponseOfBoardDetailFail() {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            return httpResponseMessage;
        }

        private HttpClient CreateMockClient()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Get, "https://DomainURL.atlassian.net/rest/agile/1.0/board")
                .Respond("application/json", getResponseOfBoardDetail());
            mockHttp.Fallback.WithAny().Respond(HttpStatusCode.BadRequest);
            

            return mockHttp.ToHttpClient();
        }

        [Fact]
        public async void TestGetBoardDetailSuccess()
        {
            List<BoardDetailDto> response = await _jiraService.getBoardDetail(jiraLoginDto);
            Assert.Equal(1, response[0].id);
            Assert.Equal("PD board", response[0].name);
            Assert.Equal(2, response[1].id);
            Assert.Equal("TFJA board", response[1].name);
        
        }

        [Fact]
        public async void TestGetBoardDetailFail()
        {
            await Assert.ThrowsAsync<Exception>(() => ( _jiraService.getBoardDetail(dtoForFail)));
        }

        [Fact]
        public void TestCreateJiraRepoSuccess()
        {
            mockJiraService.Setup(p => p.getBoardDetail(jiraLoginDto)).ReturnsAsync(boardDetailDtos);
            var exception = Record.Exception(() =>  _jiraService.CreateJiraRepo(jiraLoginDto));
            Assert.Null(exception);
        }

        [Fact]
        public void TestGetBoardDetailNullDataThrowExcepton()
        {
            var exception = Record.ExceptionAsync(async () => await _jiraService.getBoardDetail(jiroLoginDtoNull));
            Assert.NotNull(exception);
        }

        [Fact]
        public void TestGetBoardDetailDomainURLIsEmptyStringThrowExcepton()
        {
            var exception = Record.ExceptionAsync(async () => await _jiraService.getBoardDetail(jiraLoginDtoNoData));
            Assert.NotNull(exception);
        }
        
    }
}