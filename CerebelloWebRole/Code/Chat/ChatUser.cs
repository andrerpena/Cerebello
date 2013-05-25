namespace CerebelloWebRole.Code.Chat
{
    public class ChatUser
    {
        public enum StatusType
        {
            Offline,
            Online
        }

        public ChatUser()
        {
            this.Status = StatusType.Offline;
        }

        /// <summary>
        /// User Id (the same as the database user Id)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        public string ProfilePictureUrl { get; set; }

        /// <summary>
        /// The user's status
        /// </summary>
        public StatusType Status { get; set; }
    }
}