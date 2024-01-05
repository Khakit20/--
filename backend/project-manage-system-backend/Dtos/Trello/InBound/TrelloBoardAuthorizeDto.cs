namespace project_manage_system_backend.Dtos
{

    public class TrelloBoardAuthorizeDto
    {
        /// <summary>
        /// 列表ID
        /// </summary>
        public string BoardId { get; set; }

        /// <summary>
        /// 使用者ID，用於去DataBase 撈 key token
        /// </summary>
        public string Account { get; set; }
    }
}
