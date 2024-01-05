namespace project_manage_system_backend.Dtos
{

    public class TrelloInboundAddCardDto
    {
        /// <summary>
        /// Card的名子
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 要新增的列表Id
        /// </summary>
        public string ListId { get; set; }
    }
}
