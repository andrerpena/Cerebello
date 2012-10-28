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

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="roomId">The room Id (practice id)</param>
        ///// <param name="onRoomChanged">
        ///// A method that will be called when the state of the room changes, that means,
        ///// when somebody gets in or out. 
        ///// </param>
        ///// <param name="noWait"> </param>
        //public static void WaitForRoomUsersChanged(int roomId, Action<List<ChatUser>> onRoomChanged, bool noWait = false)
        //{
        //    // schedules the passed action for execution as soon as possible by any thread within the 
        //    // thread pool.
        //    // this method triggers a NotSupportedException in case of failure.
        //    if (!Rooms.ContainsKey(roomId))
        //        throw new Exception("The room does not exist");

        //    var room = Rooms[roomId];

        //    // this code will wait for MAX_WAIT_SECONDS (60 seconds) until some modification
        //    // happens in the room (someone enters or leaves). As soon as the room changes,
        //    // all users will be notified. In the other hand, if no changes happen in the room
        //    // for MAX_WAIT_SECONDS, the current state of the room is passed in to the client any way

        //    List<ChatUser> usersInRoom = null;

        //    // this will lock this thread until it's signaled. 
        //    var wait = new AutoResetEvent(false);
        //    using (room.SubscribeForUsersChange((r, chatUser, action, users) =>
        //        {
        //            usersInRoom = users;
        //            // this will release this Thread
        //            wait.Set();
        //        }))
        //    {
        //        // this thread should get stuck here until wait.Set() is called
        //        wait.WaitOne(noWait ? TimeSpan.FromSeconds(0) : TimeSpan.FromSeconds(MAX_WAIT_SECONDS));
        //    };

        //    // here, if usersInRoom has been set already, that means there was not changes in the room
        //    // in 60 seconds, let's just get the room state and return to the client

        //    if (usersInRoom == null)
        //        usersInRoom = room.GetUsers();

        //    onRoomChanged(usersInRoom);
        //}

        //public static void WaitForNewMessage(int roomId, int myUserId, Action<ChatMessage> onNewMessage)
        //{
        //    if (!Rooms.ContainsKey(roomId))
        //        throw new Exception("The room does not exist");

        //    var room = Rooms[roomId];

        //    var wait = new AutoResetEvent(false);
        //    ChatMessage newMessage = null;

        //    using (room.SubscribeForMessagesChange((r, userFrom, userTo, message) =>
        //        {
        //            // all messages destinated to the given user must be considered
        //            if (userTo.Id != myUserId)
        //                return;
        //            onNewMessage(message);
        //            wait.Set();
        //        }))
        //    {
        //        // this thread should get stuck here until wait.Set() is called
        //        wait.WaitOne(TimeSpan.FromSeconds(MAX_WAIT_SECONDS));
        //    };

        //    // this newMessage can be null, meaning that there's no new message
        //    onNewMessage(null);
        //}
    }
}