using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.Chat
{
    /// <summary>
    /// Allows for single time chatroom event subscription 
    /// </summary>
    public class ChatRoomUsersSubscription : IDisposable
    {
        public ChatRoom Room { get; private set; }
        public Action<int, List<ChatUser>> Action { get; private set; }

        public ChatRoomUsersSubscription(ChatRoom room, Action<int, List<ChatUser>> action)
        {
            this.Room = room;
            this.Action = action;

            this.Room.UserStatusChanged += this.Action;
        }

        public void Dispose()
        {
            this.Room.UserStatusChanged -= this.Action;
        }
    }
}