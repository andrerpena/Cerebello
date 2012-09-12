using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code.Chat;
using Cerebello.Model;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ChatController : AsyncController
    {
        /// <summary>
        /// Sets a user offline
        /// </summary>
        /// <remarks>
        /// ToDo: This action has a hole. As anyone can just call it to make a friend 
        /// off. The good side is that, if the person is really on, he/she will automatically be back on
        /// in a few seconds.
        /// </remarks>
        public void SetUserOffline(int roomId, int userId)
        {
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
        [AsyncTimeout(int.MaxValue)]
        public void UserListAsync(int roomId, int userId, bool noWait = false)
        {
            this.AsyncManager.OutstandingOperations.Increment();

            // if either the room or the user is not set up yet, do it
            this.SetupRoomIfNonexisting(roomId);
            this.SetupUserIfNonexisting(roomId, userId);

            // updates the status of the current user
            ChatServer.Rooms[roomId].SetUserOnline(userId);


            // this is the async operation
            ChatServer.CheckForRoomUsersChanged(roomId, users =>
            {
                // this is necessary to remove the current user from
                // the buddy list before retrieving
                var roomUsersExcludingCurrentUser = users.Where(u => u.Id != userId).OrderBy(u => u.Name).ToList();
                this.AsyncManager.Parameters["users"] = roomUsersExcludingCurrentUser;
                AsyncManager.OutstandingOperations.Decrement();
            }, noWait);
        }

        [HttpGet]
        public JsonResult UserListCompleted(List<ChatUser> users)
        {
            return this.Json(users, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AsyncTimeout(int.MaxValue)]
        public void GetMessagesAsync(int roomId, int myUserId, long? timeStamp = null)
        {
            this.AsyncManager.OutstandingOperations.Increment();

            // if either the room or the user is not set up yet, do it
            this.SetupRoomIfNonexisting(roomId);
            this.SetupUserIfNonexisting(roomId, myUserId);

            var existingMessages = ChatServer.Rooms[roomId].GetMessages(myUserId, timeStamp);
            // if there are messages aleady, return them
            if (existingMessages.Any())
            {
                // it's the first time the user is requesting messages, so return ALL of them, including the ones 
                // the current user sent him(her)self
                this.AsyncManager.Parameters["messages"] = existingMessages;
                this.AsyncManager.Parameters["fromCache"] = true;
                AsyncManager.OutstandingOperations.Decrement();
            }
            else
            {
                // .. otherwise, lets WAIT for a new message and return it when it comes
                ChatServer.CheckForNewMessage(roomId, myUserId, m =>
                {
                    var newMessages = new List<ChatMessage>();
                    if (m != null)
                        newMessages.Add(m);
                    // messages will be empty if there's no new message
                    this.AsyncManager.Parameters["messages"] = newMessages;
                    AsyncManager.OutstandingOperations.Decrement();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="fromCache">
        /// Indicates whether or not the message is new. New messages (fromCache = false) will
        /// cause the correspondig chat window to open. Otherwise it won't.
        /// </param>
        /// <returns></returns>
        public JsonResult GetMessagesCompleted(List<ChatMessage> messages, bool? fromCache = false)
        {
            return this.Json(new
            {
                Messages = messages,
                // there's a problem here. Messages that happened to arrive 
                // exactly in the time window between the time I checked 
                // and now will not be retrieved.
                // But I cannot handle all these details now or I'll never
                // get done with this.
                // ToDo: fix this
                Timestamp = DateTime.UtcNow.Ticks.ToString(),
                FromCache = fromCache
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult NewMessage(int roomId, int myUserId, int otherUserId, string message)
        {
            //ToDo: store this message in the DB.
            this.SetupRoomIfNonexisting(roomId);
            this.SetupUserIfNonexisting(roomId, myUserId);
            this.SetupUserIfNonexisting(roomId, otherUserId);

            ChatServer.Rooms[roomId].AddMessage(myUserId, otherUserId, message);

            return null;
        }


        /// <summary>
        /// Lock target for creating rooms (to prevent concurrent threads to create the same room more than once)
        /// </summary>
        public static object roomLock = new Object();

        /// <summary>
        /// Sets up a room if it does not exist
        /// </summary>
        /// <param name="roomId"></param>
        public void SetupRoomIfNonexisting(int roomId)
        {
            lock (roomLock)
            {
                // if the given room hasn't been set up yet, it must be done now
                if (!ChatServer.RoomExists(roomId))
                {
                    // creates the chat room
                    var newChatRoom = new ChatRoom(roomId);
                    ChatServer.Rooms.Add(roomId, newChatRoom);

                    using (var db = new CerebelloEntities())
                    {
                        // var practiceUsers = practice.Users.OrderBy(u => u.Person.FullName);
                        var practiceUsers = db.Users.Where(u => u.PracticeId == roomId).OrderBy(u => u.Person.FullName);

                        // adds users to the room
                        foreach (var u in practiceUsers)
                        {
                            newChatRoom.Users.Add(u.Id, new ChatUser()
                            {
                                Id = u.Id,
                                Name = u.Person.FullName,
                                Status = ChatUser.StatusType.Offline
                            });
                        }
                    }
                }
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
        public void SetupUserIfNonexisting(int roomId, int userId)
        {
            lock (userLock)
            {
                if (!ChatServer.Rooms[roomId].UserExists(userId))
                {
                    using (var db = new CerebelloEntities())
                    {
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
    }
}