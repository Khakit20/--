namespace project_manage_system_backend.Dtos
{

    public class TrelloListAuthorizeDto
    {
        /// <summary>
        /// 勘版ID
        /// </summary>
        public string ListId { get; set; }

        /// <summary>
        /// 使用者帳號，用於去DataBase 撈trello key token
        /// </summary>
        public string Account { get; set; }
    }
}
