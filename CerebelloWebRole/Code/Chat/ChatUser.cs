using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        /// <summary>
        /// Profile Url
        /// </summary>
        public string Url { get; set; }

        public string ProfilePictureUrl { get; set; }

        /// <summary>
        /// The user's status
        /// </summary>
        public StatusType Status { get; set; }

        /// <summary>
        /// Last time (UTC) this user has been active. Being active here does not mean send messages or something.
        /// Being active means having an open browser requesting the server.
        /// </summary>
        public DateTime? LastActiveOn { get; set; }
    }
}