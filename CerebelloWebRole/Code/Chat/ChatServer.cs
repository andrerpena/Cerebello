using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace CerebelloWebRole.Code.Chat
{
    public static class ChatServer
    {
        //private const int MAX_WAIT_SECONDS = 30;

        static ChatServer()
        {
            Rooms = new Dictionary<int, ChatRoom>();
        }

        public static Dictionary<int, ChatRoom> Rooms { get; private set; }

        /// <summary>
        /// Returns whether or not the given room
        /// </summary>
        /// <param name="roomId"> </param>
        public static bool RoomExists(int roomId)
        {
            return Rooms.ContainsKey(roomId);
        }
    }
}