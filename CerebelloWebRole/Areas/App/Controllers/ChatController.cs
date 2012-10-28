using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code.Chat;
using Cerebello.Model;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ChatController : PracticeController
    {
        /// <summary>
        /// Sets a user offline
        /// </summary>
        /// <remarks>
        /// ToDo: This action has a hole. As anyone can just call it to make a friend 
        /// off. The good side is that, if the person is really on, he/she will automatically be back on
        /// in a few seconds.
        /// </remarks>
        public void SetUserOffline(int userId)
        {
            var roomId = this.Practice.Id;

            if (ChatServer.RoomExists(roomId) && ChatServer.Rooms[roomId].UserExists(userId))
                ChatServer.Rooms[roomId].SetUserOffline(userId);
        }

        ///// <summary>
        ///// Returns the buddy-list (as facebook calls it), the list of users in the current chatroom
        ///// This Action should also inform the ChatServer that the user is active
        ///// </summary>
        ///// <param name="roomId"></param>
        ///// <param name="userId"></param>
        ///// <param name="noWait">
        ///// Indicates whether it should wait for room modifications or just return it right away
        ///// </param>
        //[HttpGet]
        //public JsonResult UserList(bool noWait = false)
        //{
        //    var roomId = this.Practice.Id;
        //    var myUserId = this.DbUser.Id;

        //    // if either the room or the user is not set up yet, do it
        //    ChatServerHelper.SetupRoomIfNonexisting(this.db, roomId);
        //    ChatServerHelper.SetupUserIfNonexisting(this.db, roomId, myUserId);

        //    // updates the status of the current user
        //    ChatServer.Rooms[roomId].SetUserOnline(myUserId);
        //    List<ChatUser> result = new List<ChatUser>();

        //    // this is the async operation
        //    ChatServer.WaitForRoomUsersChanged(roomId, users =>
        //    {
        //        // this is necessary to remove the current user from
        //        // the buddy list before retrieving
        //        var roomUsersExcludingCurrentUser = users.Where(u => u.Id != myUserId).OrderBy(u => u.Name).ToList();
        //        result = roomUsersExcludingCurrentUser;
        //    }, noWait);

        //    return this.Json(result, JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet]
        //public JsonResult GetMessages(long? timeStamp = null)
        //{
        //    var roomId = this.Practice.Id;
        //    var myUserId = this.DbUser.Id;

        //    // if either the room or the user is not set up yet, do it
        //    ChatServerHelper.SetupRoomIfNonexisting(this.db,roomId);
        //    ChatServerHelper.SetupUserIfNonexisting(this.db,roomId, myUserId);

        //    // Each UserFrom Id has a LIST of messages. Of course
        //    // all messages have the same UserTo, of course, myUserId.
        //    Dictionary<string, List<CerebelloWebRole.Code.Chat.ChatMessage>> messages = new Dictionary<string, List<CerebelloWebRole.Code.Chat.ChatMessage>>();

        //    // possible existing messages
        //    var existingMessages = timeStamp.HasValue ? ChatServer.Rooms[roomId].GetMessagesTo(myUserId, timeStamp.Value) : new List<CerebelloWebRole.Code.Chat.ChatMessage>();

        //    if (timeStamp.HasValue && existingMessages.Any())
        //    {
        //        // makes the messages follow the scructure: Each UserFrom Id has a LIST of messages
        //        messages = existingMessages.GroupBy(cm => cm.UserFrom.Id).ToDictionary(g => g.Key.ToString(), g => g.ToList());
        //    }
        //    else
        //    {
        //        // .. otherwise, lets WAIT for a new message and return it when it comes
        //        ChatServer.WaitForNewMessage(roomId, myUserId, m =>
        //        {
        //            if (m != null)
        //                messages.Add(m.UserFrom.Id.ToString(), new List<CerebelloWebRole.Code.Chat.ChatMessage>() { m });
        //        });
        //    }

        //    return this.Json(new
        //    {
        //        Messages = messages,
        //        Timestamp = DateTime.UtcNow.Ticks.ToString()
        //    }, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public JsonResult GetMessageHistory(int otherUserId, long? timeStamp = null)
        {
            var roomId = this.Practice.Id;
            var myUserId = this.DbUser.Id;

            ChatServerHelper.SetupRoomIfNonexisting(this.db,roomId);
            ChatServerHelper.SetupUserIfNonexisting(this.db,roomId, myUserId);
            ChatServerHelper.SetupUserIfNonexisting(this.db,roomId, otherUserId);

            // Each UserFrom Id has a LIST of messages. Of course
            // all messages have the same UserTo, of course, myUserId.
            Dictionary<string, List<CerebelloWebRole.Code.Chat.ChatMessage>> messages = new Dictionary<string, List<CerebelloWebRole.Code.Chat.ChatMessage>>();
            messages.Add(otherUserId.ToString(), ChatServer.Rooms[roomId].GetMessagesBetween(myUserId, otherUserId, timeStamp));

            return this.Json(new
            {
                Messages = messages,
                Timestamp = DateTime.UtcNow.Ticks.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult NewMessage(int otherUserId, string message)
        {
            var myUserId = this.DbUser.Id;

            if (myUserId == otherUserId)
                throw new Exception("Cannot send a message to yourself");

            var roomId = this.Practice.Id;

            //ToDo: store this message in the DB.
            ChatServerHelper.SetupRoomIfNonexisting(this.db, roomId);
            ChatServerHelper.SetupUserIfNonexisting(this.db, roomId, myUserId);
            ChatServerHelper.SetupUserIfNonexisting(this.db, roomId, otherUserId);

            ChatServer.Rooms[roomId].AddMessage(myUserId, otherUserId, message);

            // new let's try to persist it

            db.ChatMessages.AddObject(new Cerebello.Model.ChatMessage()
            {
                Date = DateTime.UtcNow,
                Message = message,
                UserFromId = myUserId,
                UserToId = otherUserId,
                PracticeId = roomId
            });

            db.SaveChanges();

            return null;
        }
    }
}