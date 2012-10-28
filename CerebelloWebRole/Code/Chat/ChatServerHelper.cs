using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cerebello.Model;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Chat
{
    public class ChatServerHelper
    {
        /// <summary>
        /// Lock target for creating rooms (to prevent concurrent threads to create the same room more than once)
        /// </summary>
        private static readonly object userLock = new Object();

        /// <summary>
        /// Lock target for creating rooms (to prevent concurrent threads to create the same room more than once)
        /// </summary>
        private static readonly object roomLock = new Object();


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
        /// Sets up an user if it does not exist
        /// </summary>
        /// <param name="db"></param>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        public static ChatUser SetupUserIfNonexisting([NotNull] CerebelloEntities db, int roomId, int userId)
        {
            if (db == null) throw new ArgumentNullException("db");
            lock (userLock)
            {
                if (!ChatServer.RoomExists(roomId))
                    throw new Exception("Room does not exist. Room Id: " + roomId);

                if (ChatServer.Rooms[roomId].UserExists(userId))
                    return ChatServer.Rooms[roomId].Users[userId];

                var user = db.Users.Include("Person").FirstOrDefault(u => u.Id == userId);

                var newUser = new ChatUser()
                    {
                        Id = user.Id,
                        Name = user.Person.FullName
                    };

                // in the case the current user does not exist, it has been added after the room has been set up.
                ChatServer.Rooms[roomId].AddUser(newUser);
                return newUser;
            }
        }

        /// <summary>
        /// Sets up a room if it does not exist
        /// </summary>
        /// <param name="db"></param>
        /// <param name="roomId"></param>
        public static ChatRoom SetupRoomIfNonexisting([NotNull] CerebelloEntities db, int roomId)
        {
            if (db == null) throw new ArgumentNullException("db");
            lock (roomLock)
            {
                // if the given room hasn't been set up yet, it must be done now
                if (ChatServer.RoomExists(roomId))
                    return ChatServer.Rooms[roomId];
                // creates the chat room
                var newChatRoom = new ChatRoom(roomId);
                ChatServer.Rooms.Add(roomId, newChatRoom);

                // var practiceUsers = practice.Users.OrderBy(u => u.Person.FullName);
                var practiceUsers = db.Users.Where(u => u.PracticeId == roomId).OrderBy(u => u.Person.FullName);

                // adds users to the room
                foreach (var u in practiceUsers)
                {
                    newChatRoom.Users.Add(u.Id, GetChatUserFromUser(u));
                }

                // now adds conversations to the history

                newChatRoom.Messages.AddRange((from m in db.ChatMessages
                                               where m.PracticeId == roomId
                                               orderby m.Date descending
                                               select m
                                              ).ToList().Select(m => new ChatMessage()
                                              {
                                                  UserFrom = GetChatUserFromUser(m.UserTo),
                                                  UserTo = GetChatUserFromUser(m.UserFrom),
                                                  Message = m.Message,
                                                  Timestamp = m.Date.Ticks
                                              }).Take(400).AsEnumerable().Reverse());

                return newChatRoom;
            }
        }
    }
}