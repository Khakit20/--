namespace project_manage_system_backend.Dtos
{

    public class TrelloListsInfoDto
    {
        /// <summary>
        /// ListID
        /// </summary>
        public string ListId { get { return id; } set { id = value; } }

        /// <summary>
        /// List名稱
        /// </summary>
        public string ListName { get { return name; } set { name = value; } }

        /// <summary>
        /// ListID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// List名稱
        /// </summary>
        public string name { get; set; }
    }
}
