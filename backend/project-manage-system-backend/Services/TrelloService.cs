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
using Newtonsoft.Json;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Net;
using System.Reflection;
using project_manage_system_backend.EAS;

namespace project_manage_system_backend.Services
{
    public class TrelloService : BaseService
    {

        private readonly TrelloEAS _trelloEAS;
        public TrelloService(IConfiguration configuration, PMSContext dbContext, HttpClient client = null) : base(dbContext)
        {
            _configuration = configuration;
            _trelloEAS = new TrelloEAS(client ?? new HttpClient());
        }

        // 實作檢查user目前是否有綁定Trello資訊
        public void CheckUserBindStatus(string userId)
        {
            var user = _dbContext.Users.Include(u => u.Projects).ThenInclude(p => p.Project).FirstOrDefault(u => u.Account.Equals(userId));
            Console.WriteLine("[CheckUserBindStatus] Searching User");
            if (user != null)
            {
                Console.WriteLine("[CheckUserBindStatus] Find user");
                if (user.TrelloKey == null || user.TrelloToken == null)
                {
                    Console.WriteLine("[CheckUserBindStatus] User Still Not Bind Trello Key Token!");
                    throw new Exception("Can't Find Key And Token");
                }
            }
            else
            {
                Console.WriteLine("[ClearTrelloBindInfo] Can't Find This User");
                throw new Exception("Can't Find This User");
            }
        }

        // 實作清理user綁定的Trello資訊
        public void ClearTrelloBindInfo(string userId)
        {
            var user = _dbContext.Users.Include(u => u.Projects).ThenInclude(p => p.Project).FirstOrDefault(u => u.Account.Equals(userId));
            Console.WriteLine("[ClearTrelloBindInfo] Searching User");
            if (user != null)
            {
                Console.WriteLine("[ClearTrelloBindInfo] Find User");
                user.TrelloKey = null;
                user.TrelloToken = null;
                _dbContext.SaveChanges();
                Console.WriteLine("[ClearTrelloBindInfo] Clear Success");
            }
            else
            {
                Console.WriteLine("[ClearTrelloBindInfo] Can't Find This User");
                throw new Exception("Can't Find This User");
            }
        }

        // 實作將trello key和token綁定給user
        public void BindUserToTrello(TrelloBindInfoDto dto, string userId)
        {
            var user = _dbContext.Users.Include(u => u.Projects).ThenInclude(p => p.Project).FirstOrDefault(u => u.Account.Equals(userId));
            if (user != null)
            {
                user.TrelloKey = dto.Key;
                user.TrelloToken = dto.Token;
                if (_dbContext.SaveChanges() == 0)
                {
                    Console.WriteLine("[BindUserToTrello] Update User Fail");
                    throw new Exception("Update User Fail");
                }
            }
            else
            {
                Console.WriteLine("[BindUserToTrello] Can't Find This User");
                throw new Exception("Can't Find This User");
            }
        }

        // 實作檢查trello的key和token的合法性
        public async Task<bool> CheckTrelloBindInfoExist(string key, string token)
        {
            string defaultUrl = $"https://api.trello.com/1/search/members/?query=1&key={key}&token={token}";
            bool result = await _trelloEAS.CheckStatusCode(defaultUrl);
            return result;
        }

        // 根據url取得trello boardId
        public async Task<string> GetBoardIdByUrl(string url, string key, string token)
        {
            string jsonUrl = url + ".json?" + $"key={key}&token={token}";
            try
            {
                TrelloBoardCheckInfoDto result = await _trelloEAS.GetDataSingle<TrelloBoardCheckInfoDto>(jsonUrl);
                return result.id;
            }
            catch (Exception ex)
            {
                return "None";
            }
        }

        // 實作檢查該trello的boardId是否跟key和token符合
        public async Task<bool> CheckTrelloRepoIsExist(string boardId, string key, string token)
        {
            string defaultUrl = $"https://api.trello.com/1/boards/{boardId}?key={key}&token={token}";
            bool result = await _trelloEAS.CheckStatusCode(defaultUrl);
            return result;
        }

        // 實作檢查boardId跟user綁定的trello key和token符合
        public async Task<bool> CheckTrelloRepo(TrelloLoginDto dto, string userId)
        {
            var user = _dbContext.Users.Include(u => u.Projects).ThenInclude(p => p.Project).FirstOrDefault(u => u.Account.Equals(userId));
            if (user == null)
            {
                Console.WriteLine("[CheckTrelloRepo] Can't Find This User");
                throw new Exception("Can't Find This User");
            }
            string boardId = await GetBoardIdByUrl(dto.BoardUrl, user.TrelloKey, user.TrelloToken);
            if (boardId == "None")
            {
                Console.WriteLine("[CheckTrelloRepo] Invalid BoardUrl");
                throw new Exception("Invalid BoardUrl");
            }
            bool result = await CheckTrelloRepoIsExist(boardId, user.TrelloKey, user.TrelloToken);
            if (result)
            {
                Console.WriteLine("[CheckTrelloRepo] Trello Repo Info Correct");
                return true;
            }
            else
            {
                Console.WriteLine("[CheckTrelloRepo] Can't Find Repo");
                throw new Exception("Can't Find Repo");
            }
        }

        // 實作將user新增的trello repo資料儲存起來
        public async Task<bool> CreateTrelloRepo(TrelloLoginDto dto, string userId)
        {
            var user = _dbContext.Users.Include(u => u.Projects).ThenInclude(p => p.Project).FirstOrDefault(u => u.Account.Equals(userId));
            if (user == null)
            {
                Console.WriteLine("[CreateTrelloRepo] Can't Find This User");
                throw new Exception("Can't Find This User");
            }
            string boardId = await GetBoardIdByUrl(dto.BoardUrl, user.TrelloKey, user.TrelloToken);
            if (boardId == "None")
            {
                Console.WriteLine("[CreateTrelloRepo] Invalid BoardUrl");
                throw new Exception("Invalid BoardUrl");
            }
            var project = _dbContext.Projects.Where(p => p.ID == dto.ProjectId).First();
            Repo repo = new Repo
            {
                Name = dto.Name, // TODO: IsSonarqube要拿掉
                IsSonarqube = false,
                Project = project,
                BoardId = boardId,
                Type = "Trello"
            };
            _dbContext.Repositories.Add(repo);
            if (_dbContext.SaveChanges() == 0)
            {
                Console.WriteLine("[CreateTrelloRepo] Create Repository Fail");
                throw new Exception("Create Repository Fail");
            }
            return true;
        }

        /// <summary>
        /// 取得board底下的所有工作列表
        /// </summary>
        /// <param name="trelloAuthorizeDto"></param>
        /// <returns></returns>
        public async Task<List<TrelloListsInfoDto>> GetListsInfo(TrelloBoardAuthorizeDto trelloAuthorizeDto, string userId)
        {
            try
            {
                string apiUrl = _configuration.GetValue<string>("TrelloAPI:APIUrl");
                (string key, string token) = GetKeyToken(userId);

                string url = $"{apiUrl}/boards/{trelloAuthorizeDto.BoardId}/lists?key={key}&token={token}";

                return await _trelloEAS.GetDataArray<TrelloListsInfoDto>(url);
            }
            catch
            {
                return new List<TrelloListsInfoDto>();
            }
        }

        /// <summary>
        /// 取得該列表底下的所有Card
        /// </summary>
        /// <param name="trelloAuthorizeDto"></param>
        /// <returns></returns>
        public async Task<List<TrelloCardsInlistDto>> GetCardsInList(TrelloListAuthorizeDto trelloAuthorizeDto, string userId)
        {
            try
            {
                string apiUrl = _configuration.GetValue<string>("TrelloAPI:APIUrl");
                (string key, string token) = GetKeyToken(userId);

                string url = $"{apiUrl}/lists/{trelloAuthorizeDto.ListId}/cards?key={key}&token={token}";

                return await _trelloEAS.GetDataArray<TrelloCardsInlistDto>(url);
            }
            catch
            {
                throw new HttpRequestException("get trello cards fail");
            }
        }

        /// <summary>
        /// 新增卡片
        /// </summary>
        /// <param name="trelloAddCardDto"></param>
        /// <returns></returns>
        public async Task<bool> AddCardsInList(TrelloInboundAddCardDto trelloAddCardDto, string userId)
        {
            TrelloOutboundAddCardDto trelloOutboundAddCard = new TrelloOutboundAddCardDto()
            {
                name = trelloAddCardDto.Name,
            };
            string apiUrl = _configuration.GetValue<string>("TrelloAPI:APIUrl");

            (string key, string token) = GetKeyToken(userId);

            string url = $"{apiUrl}/cards?idList={trelloAddCardDto.ListId}&key={key}&token={token}";
            return await _trelloEAS.PostData(url, trelloOutboundAddCard);
        }

        /// <summary>
        /// 從資料庫取的Key和Token
        /// </summary>
        /// <returns></returns>
        private (string, string) GetKeyToken(string userId)
        {
            User user = _dbContext.Users.Include(u => u.Projects).ThenInclude(p => p.Project).FirstOrDefault(u => u.Account.Equals(userId));
            string key = user.TrelloKey;
            string token = user.TrelloToken;
            return (key, token);
        }

        public async Task<TrelloCardInfoDto> GetTrelloCardInfo(TrelloCard trelloCard, string userId)
        {
            try
            {
                string apiUrl = _configuration.GetValue<string>("TrelloAPI:APIUrl");
                (string key, string token) = GetKeyToken(userId);

                string url = $"{apiUrl}/cards/{trelloCard.CardId}?key={key}&token={token}";

                return await _trelloEAS.GetDataSingle<TrelloCardInfoDto>(url);
            }
            catch
            {
                return new TrelloCardInfoDto();
            }
        }

        public async Task<bool> UpdateTrelloCardInfo(TrelloCardUpdateDto trelloCard, string userId)
        {
            try
            {
                TrelloCardInfoUpdateDto trelloCardInfoUpdate = new TrelloCardInfoUpdateDto()
                {
                    desc = trelloCard.Desc,
                    idList = trelloCard.ListId,
                    name = trelloCard.CardName
                };
                string apiUrl = _configuration.GetValue<string>("TrelloAPI:APIUrl");
                (string key, string token) = GetKeyToken(userId);

                string url = $"{apiUrl}/cards/{trelloCard.CardId}?key={key}&token={token}";

                return await _trelloEAS.PutData<TrelloCardInfoUpdateDto>(url, trelloCardInfoUpdate);
            }
            catch
            {
                return false;
            }
        }
    }
}