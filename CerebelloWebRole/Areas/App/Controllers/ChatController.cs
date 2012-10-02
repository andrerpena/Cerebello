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

        /// <summary>
        /// Returns the buddy-list (as facebook calls it), the list of users in the current chatroom
        /// This Action should also inform the ChatServer that the user is active
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        /// <param name="noWait">
        /// Indicates whether it should wait for room modifications or just return it right away
        /// </param>
        [HttpGet]
        // this is for debug purpose only
        public JsonResult UserList(bool noWait = false)
        {
            var roomId = this.Practice.Id;
            var myUserId = this.DbUser.Id;

            // if either the room or the user is not set up yet, do it
            this.SetupRoomIfNonexisting(roomId);
            this.SetupUserIfNonexisting(roomId, myUserId);

            // updates the status of the current user
            ChatServer.Rooms[roomId].SetUserOnline(myUserId);
            List<ChatUser> result = new List<ChatUser>();

            // this is the async operation
            ChatServer.WaitForRoomUsersChanged(roomId, users =>
            {
                // this is necessary to remove the current user from
                // the buddy list before retrieving
                var roomUsersExcludingCurrentUser = users.Where(u => u.Id != myUserId).OrderBy(u => u.Name).ToList();
                result = roomUsersExcludingCurrentUser;
            }, noWait);

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMessages(long? timeStamp = null)
        {
            var roomId = this.Practice.Id;
            var myUserId = this.DbUser.Id;

            // if either the room or the user is not set up yet, do it
            this.SetupRoomIfNonexisting(roomId);
            this.SetupUserIfNonexisting(roomId, myUserId);

            // Each UserFrom Id has a LIST of messages. Of course
            // all messages have the same UserTo, of course, myUserId.
            Dictionary<string, List<CerebelloWebRole.Code.Chat.ChatMessage>> messages = new Dictionary<string, List<CerebelloWebRole.Code.Chat.ChatMessage>>();

            // possible existing messages
            var existingMessages = timeStamp.HasValue ? ChatServer.Rooms[roomId].GetMessagesTo(myUserId, timeStamp.Value) : new List<CerebelloWebRole.Code.Chat.ChatMessage>();

            if (timeStamp.HasValue && existingMessages.Any())
            {
                // makes the messages follow the scructure: Each UserFrom Id has a LIST of messages
                messages = existingMessages.GroupBy(cm => cm.UserFrom.Id).ToDictionary(g => g.Key.ToString(), g => g.ToList());
            }
            else
            {
                // .. otherwise, lets WAIT for a new message and return it when it comes
                ChatServer.WaitForNewMessage(roomId, myUserId, m =>
                {
                    if (m != null)
                        messages.Add(m.UserFrom.Id.ToString(), new List<CerebelloWebRole.Code.Chat.ChatMessage>() { m });
                });
            }

            return this.Json(new
            {
                Messages = messages,
                Timestamp = DateTime.UtcNow.Ticks.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMessageHistory(int otherUserId, long? timeStamp = null)
        {
            var roomId = this.Practice.Id;
            var myUserId = this.DbUser.Id;

            this.SetupRoomIfNonexisting(roomId);
            this.SetupUserIfNonexisting(roomId, myUserId);
            this.SetupUserIfNonexisting(roomId, otherUserId);

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
            this.SetupRoomIfNonexisting(roomId);
            this.SetupUserIfNonexisting(roomId, myUserId);
            this.SetupUserIfNonexisting(roomId, otherUserId);

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


        /// <summary>
        /// Lock target for creating rooms (to prevent concurrent threads to create the same room more than once)
        /// </summary>
        public static object roomLock = new Object();

        private static ChatUser GetChatUserFromUser(User u)
        {
            return new ChatUser()
                            {
                                Id = u.Id,
                                Name = u.Person.FullName,
                                Status = ChatUser.StatusType.Offline,
                                GravatarUrl = GravatarHelper.GetGravatarUrl(u.Person.EmailGravatarHash, GravatarHelper.Size.s32)
                            };
        }

        /// <summary>
        /// Sets up a room if it does not exist
        /// </summary>
        /// <param name="roomId"></param>
        private void SetupRoomIfNonexisting(int roomId)
        {
            lock (roomLock)
            {
                // if the given room hasn't been set up yet, it must be done now
                if (ChatServer.RoomExists(roomId)) return;
                // creates the chat room
                var newChatRoom = new ChatRoom(roomId);
                ChatServer.Rooms.Add(roomId, newChatRoom);

                // var practiceUsers = practice.Users.OrderBy(u => u.Person.FullName);
                var practiceUsers = this.db.Users.Where(u => u.PracticeId == roomId).OrderBy(u => u.Person.FullName);

                // adds users to the room
                foreach (var u in practiceUsers)
                {
                    newChatRoom.Users.Add(u.Id, GetChatUserFromUser(u));
                }

                // now adds conversations to the history

                newChatRoom.Messages.AddRange((from m in this.db.ChatMessages
                                               where m.PracticeId == roomId
                                               orderby m.Date descending
                                               select m
                                              ).ToList().Select(m => new CerebelloWebRole.Code.Chat.ChatMessage()
                                                  {
                                                      UserFrom = GetChatUserFromUser(m.UserTo),
                                                      UserTo = GetChatUserFromUser(m.UserFrom),
                                                      Message = m.Message,
                                                      Timestamp = m.Date.Ticks
                                                  }).Take(400).AsEnumerable().Reverse());
            }
        }

        /// <summary>
        /// Lock target for creating rooms (to prevent concurrent threads to create the same room more than once)
        /// </summary>
        public static object userLock = new Object();

        /// <summary>
        /// Sets up a room if it does not exist
        /// </summary>
        /// <param name="roomId"></param>
        private void SetupUserIfNonexisting(int roomId, int userId)
        {
            lock (userLock)
            {
                if (ChatServer.Rooms[roomId].UserExists(userId)) return;
                var user = db.Users.Include("Person").FirstOrDefault(u => u.Id == userId);

                // in the case the current user does not exist, it has been added after the room has been set up.
                ChatServer.Rooms[roomId].AddUser(new ChatUser()
                    {
                        Id = user.Id,
                        Name = user.Person.FullName
                    });
            }
        }
    }
}