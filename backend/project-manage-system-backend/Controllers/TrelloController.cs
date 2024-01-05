using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using Microsoft.Extensions.Configuration;


namespace project_manage_system_backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TrelloController : ControllerBase
    {
        private TrelloService _trelloService;

        //設置trello service
        public TrelloController(IConfiguration configuration,PMSContext dbContext)
        {
            _trelloService = new TrelloService(configuration, dbContext);
        }

        public void SetTrelloService(TrelloService trelloService)
        {
            _trelloService = trelloService;
        }

        // 檢查user目前的綁定狀態
        [Authorize]
        [HttpGet("checkBind")]
        public IActionResult CheckUserBindStatus()
        {
            try
            {
                _trelloService.CheckUserBindStatus(User.Identity.Name);
                Console.WriteLine("[CheckUserBindStatus] Checked Success");
                return Ok(new ResponseDto
                {
                    success = true,
                    message = "Checked Success"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CheckUserBindStatus] Checked Error: " + ex.Message);
                return Ok(new ResponseDto
                {
                    success = false,
                    message = "Checked Error: " + ex.Message
                });
            }
        }

        // 清除user目前的綁定狀態
        [Authorize]
        [HttpGet("clearTrelloBindInfo")]
        public IActionResult ClearTrelloBindInfo()
        {
            try
            {
                _trelloService.ClearTrelloBindInfo(User.Identity.Name);
                Console.WriteLine("[clearTrelloBindInfo] Clear Success");
                return Ok(new ResponseDto
                {
                    success = true,
                    message = "Clear Success"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[clearTrelloBindInfo] Clear Error: " + ex.Message);
                return Ok(new ResponseDto
                {
                    success = false,
                    message = "Clear Error: " + ex.Message
                });
            }
        }

        // 綁定Trello的key, token到User上
        [Authorize]
        [HttpPost("bind")]
        public async Task<IActionResult> BindUserToTrello(TrelloBindInfoDto trelloBindInfoDto)
        {
            // 檢查傳入的key, token是否有效
            bool result = await _trelloService.CheckTrelloBindInfoExist(trelloBindInfoDto.Key, trelloBindInfoDto.Token);
            if (result)
            {
                try
                {
                    // 將user資料進行綁定
                    _trelloService.BindUserToTrello(trelloBindInfoDto, User.Identity.Name);
                    Console.WriteLine("[BindUserToTrello] Binded Success");
                    return Ok(new ResponseDto
                    {
                        success = true,
                        message = "Binded Success"
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[BindUserToTrello] Binded Error: " + ex.Message);
                    return Ok(new ResponseDto
                    {
                        success = false,
                        message = "Binded Error: " + ex.Message
                    });
                }
            }
            else
            {
                Console.WriteLine("[BindUserToTrello] Binded Error: Invalid Key or Token");
                return Ok(new ResponseDto
                {
                    success = false,
                    message = "Binded Error: Invalid Key or Token"
                });
            }
        }

        // 用於檢查boardUrl跟目前的user儲存的key, token是否符合
        [Authorize]
        [HttpPost("checkBoardUrl")]
        public async Task<IActionResult> CheckTrelloRepo(TrelloLoginDto trelloLoginDto)
        {
            try
            {
                bool result = await _trelloService.CheckTrelloRepo(trelloLoginDto, User.Identity.Name);
                Console.WriteLine("[CheckTrelloRepo] Checked Success");
                return Ok(new ResponseDto
                {
                    success = true,
                    message = "Checked Success"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CheckTrelloRepo] Checked Error: " + ex.Message);
                return Ok(new ResponseDto
                {
                    success = false,
                    message = "Checked Error: " + ex.Message
                });
            }
        }

        // 用於創建Trello Repo, URL = host/trello/createRepo
        [Authorize]
        [HttpPost("createRepo")]
        public async Task<IActionResult> CreateTrelloRepo(TrelloLoginDto trelloLoginDto)
        {
            try
            {
                bool result = await _trelloService.CreateTrelloRepo(trelloLoginDto, User.Identity.Name);
                Console.WriteLine("[CreateTrelloRepo] Added Success");
                return Ok(new ResponseDto
                {
                    success = true,
                    message = "Added Success"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CreateTrelloRepo] Added Error: " + ex.Message);
                return Ok(new ResponseDto
                {
                    success = false,
                    message = "Added Error: " + ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost("trelloLists")]
        public async Task<IActionResult> GetTrelloLists(TrelloBoardAuthorizeDto trelloBoardAuthorizeDto)
        {
            return Ok(await _trelloService.GetListsInfo(trelloBoardAuthorizeDto, User.Identity.Name));
        }

        [Authorize]
        [HttpPost("trelloCardsInList")]
        public async Task<IActionResult> GetTrelloCardsByList(TrelloListAuthorizeDto trelloListAuthorizeDto)
        {
            return Ok(await _trelloService.GetCardsInList(trelloListAuthorizeDto, User.Identity.Name));
        }

        [Authorize]
        [HttpPost("addTrelloCardInList")]
        public async Task<IActionResult> AddTrelloCardInList(TrelloInboundAddCardDto trelloAddCardDto)
        {
            return Ok(await _trelloService.AddCardsInList(trelloAddCardDto, User.Identity.Name));
        }

        [Authorize]
        [HttpPost("trelloCardsInfo")]
        public async Task<IActionResult> GetTrelloCardsInfo(TrelloCard trelloCard)
        {
            return Ok(await _trelloService.GetTrelloCardInfo(trelloCard, User.Identity.Name));
        }

        [Authorize]
        [HttpPut("updateTrelloCard")]
        public async Task<IActionResult> UpdateTrelloCard(TrelloCardUpdateDto trelloCard)
        {
            return Ok(await _trelloService.UpdateTrelloCardInfo(trelloCard, User.Identity.Name));
        }
    }
}
