namespace project_manage_system_backend.Dtos
{

    public class TrelloCardsInlistDto
    {
        /// <summary>
        /// CardID
        /// </summary>
        public string CardId { get { return id; } set { id = value; } }

        /// <summary>
        /// Card名稱
        /// </summary>
        public string CardName { get { return name; } set { name = value; } }

        /// <summary>
        /// CardID 
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Card名稱
        /// </summary>
        public string name { get; set; }
    }
}
