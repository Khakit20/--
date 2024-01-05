namespace project_manage_system_backend.Dtos
{

    public class TrelloCardUpdateDto
    {
        /// <summary>
        /// 卡片ID
        /// </summary>
        public string CardId { get; set; }

        public string Desc { get; set; }

        public string ListId { get; set; }

        public string CardName { get; set; }
    }
}
