using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.Chat
{
    /// <summary>
    /// Allows for single time chatroom event subscription 
    /// </summary>
    public class ChatRoomMessagesSubscription : IDisposable
    {
        public ChatRoom Room { get; private set; }
        public Action<int, ChatUser, ChatUser, ChatMessage> Action { get; private set; }

        public ChatRoomMessagesSubscription(ChatRoom room, Action<int, ChatUser, ChatUser, ChatMessage> action)
        {
            this.Room = room;
            this.Action = action;

            this.Room.MessagesChanged += this.Action;
        }

        public void Dispose()
        {
            this.Room.MessagesChanged -= this.Action;
        }
    }
}