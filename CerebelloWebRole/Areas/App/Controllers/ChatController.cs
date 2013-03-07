using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CerebelloWebRole.Code.Chat;
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
            var roomId = this.DbPractice.Id;

            if (ChatServer.RoomExists(roomId) && ChatServer.Rooms[roomId].UserExists(userId))
                ChatServer.Rooms[roomId].SetUserOffline(userId);
        }

        [HttpGet]
        public JsonResult GetUserInfo(int userId)
        {
            var roomId = this.DbPractice.Id;
            var myUserId = this.DbUser.Id;

            ChatServerHelper.SetupRoomIfNonexisting(this.db, roomId);
            ChatServerHelper.SetupUserIfNonexisting(this.db, roomId, myUserId);
            ChatServerHelper.SetupUserIfNonexisting(this.db, roomId, userId);

            // this will intentionally trigger an error in case the user doesn't exist.
            // the client must treat this scenario
            return this.Json(
                new
                    {
                        User = ChatServer.Rooms[roomId].UsersById[userId]
                    },
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMessageHistory(int otherUserId, long? timeStamp = null)
        {
            var roomId = this.DbPractice.Id;
            var myUserId = this.DbUser.Id;

            ChatServerHelper.SetupRoomIfNonexisting(this.db, roomId);
            ChatServerHelper.SetupUserIfNonexisting(this.db, roomId, myUserId);
            ChatServerHelper.SetupUserIfNonexisting(this.db, roomId, otherUserId);

            // Each UserFrom Id has a LIST of messages. Of course
            // all messages have the same UserTo, of course, myUserId.
            var messages = new Dictionary<string, List<ChatMessage>>
                {
                    {otherUserId.ToString(), ChatServer.Rooms[roomId].GetMessagesBetween(myUserId, otherUserId, timeStamp)}
                };

            return this.Json(new
            {
                Messages = messages,
                Timestamp = this.GetUtcNow().Ticks.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult NewMessage(int otherUserId, string message)
        {
            var myUserId = this.DbUser.Id;

            if (myUserId == otherUserId)
                throw new Exception("Cannot send a message to yourself");

            var roomId = this.DbPractice.Id;

            //ToDo: store this message in the DB.
            ChatServerHelper.SetupRoomIfNonexisting(this.db, roomId);
            ChatServerHelper.SetupUserIfNonexisting(this.db, roomId, myUserId);
            ChatServerHelper.SetupUserIfNonexisting(this.db, roomId, otherUserId);

            ChatServer.Rooms[roomId].AddMessage(myUserId, otherUserId, message);

            // new let's try to persist it

            db.ChatMessages.AddObject(new Cerebello.Model.ChatMessage()
            {
                Date = this.GetUtcNow(),
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