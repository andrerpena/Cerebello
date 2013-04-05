using System;
using System.Collections.Generic;
using System.Linq;
using Cerebello.Model;
using CerebelloWebRole.Code.Chat;
using CerebelloWebRole.Code.Security;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR.Hubs;
using ChatMessage = CerebelloWebRole.Code.Chat.ChatMessage;

namespace CerebelloWebRole.Code.Hubs
{
    public class ChatHub : CerebelloHub
    {
        /// <summary>
        /// Current connections
        /// 1 room has many users that have many connections (2 open browsers from the same user represents 2 connections)
        /// </summary>
        private static readonly Dictionary<string, Dictionary<int, List<string>>> connections = new Dictionary<string, Dictionary<int, List<string>>>();

        /// <summary>
        /// If the specified user is connected, return information about the user
        /// </summary>
        public ChatUser GetUserInfo(int userId)
        {
            var user = this.db.Users.FirstOrDefault(u => u.Id == userId);
            return user == null ? null : GetChatUserFromUser(user);
        }

        private ChatUser GetChatUserFromUser([NotNull] User user)
        {
            if (user == null) throw new ArgumentNullException("user");
            var myRoomId = this.GetMyRoomId();
            ChatUser.StatusType userStatus;
            lock (connections)
            {
                userStatus = connections.ContainsKey(myRoomId)
                                 ? (connections[myRoomId].ContainsKey(user.Id)
                                        ? ChatUser.StatusType.Online
                                        : ChatUser.StatusType.Offline)
                                 : ChatUser.StatusType.Offline;
            }
            return new ChatUser()
                {
                    Id = user.Id,
                    Name = user.Person.FullName,
                    Status = userStatus,
                    GravatarUrl = GravatarHelper.GetGravatarUrl(user.Person.EmailGravatarHash, GravatarHelper.Size.s32)
                };
        }

        private ChatMessage GetChatMessage(Cerebello.Model.ChatMessage chatMessage, string clientGuid)
        {
            return new ChatMessage()
                {
                    Message = chatMessage.Message,
                    UserFrom = this.GetChatUserFromUser(chatMessage.UserFrom),
                    UserTo = this.GetChatUserFromUser(chatMessage.UserTo),
                    ClientGuid = clientGuid
                };
        }

        /// <summary>
        /// Returns my user id
        /// </summary>
        /// <returns></returns>
        private int GetMyUserId()
        {
            var userPrincipal = this.Context.User as AuthenticatedPrincipal;
            if (userPrincipal == null)
                throw new NotAuthorizedException();

            var userData = userPrincipal.Profile;
            return userData.Id;
        }

        private string GetMyRoomId()
        {
            var userPrincipal = this.Context.User as AuthenticatedPrincipal;
            if (userPrincipal == null)
                throw new NotAuthorizedException();

            var userData = userPrincipal.Profile;
            return userData.PracticeIdentifier;
        }

        private void BroadcastUsersList()
        {
            var myRoomId = this.GetMyRoomId();
            var connectionIds = new List<string>();
            lock (connections)
            {
                if (connections.ContainsKey(myRoomId))
                    connectionIds = connections[myRoomId].Keys.SelectMany(userId => connections[myRoomId][userId]).ToList();
            }

            foreach (var connectionId in connectionIds)
                this.Clients.Client(connectionId).usersListChanged(this.GetUsersList());
        }

        private void BroadcastMessage(int otherUserId, Cerebello.Model.ChatMessage dbChatMessage, string clientGuid)
        {
            var myUserId = this.GetMyUserId();
            var myRoomId = this.GetMyRoomId();
            var connectionIds = new List<string>();
            lock (connections)
            {
                if (connections[myRoomId].ContainsKey(otherUserId))
                    connectionIds.AddRange(connections[myRoomId][otherUserId]);
                if (connections[myRoomId].ContainsKey(myUserId))
                    connectionIds.AddRange(connections[myRoomId][myUserId]);
            }
            foreach (var connectionId in connectionIds)
                this.Clients.Client(connectionId).newMessage(this.GetChatMessage(dbChatMessage, clientGuid));
        }

        public List<ChatUser> GetUsersList()
        {
            var myRoomId = this.GetMyRoomId();
            var practiceId = this.db.Practices.Where(p => p.UrlIdentifier == myRoomId).Select(p => p.Id).FirstOrDefault();
            var roomUsers = this.db.Users.Where(u => u.PracticeId == practiceId).OrderBy(u => u.Person.FullName).ToList();

            // now we have to see the users who are online and those who are not
            return roomUsers.Select(this.GetChatUserFromUser).ToList();
        }

        /// <summary>
        /// Returns the message history
        /// </summary>
        public List<ChatMessage> GetMessageHistory(int otherUserId)
        {
            var myUserId = this.GetMyUserId();
            var dbMessages = this.db.ChatMessages
                               .Where(
                                   m =>
                                   (m.UserTo.Id == myUserId && m.UserFrom.Id == otherUserId) ||
                                   (m.UserTo.Id == otherUserId && m.UserFrom.Id == myUserId))
                               .OrderByDescending(m => m.Date).Take(30).ToList();
            dbMessages.Reverse();
            return dbMessages.Select(m => this.GetChatMessage(m, null)).ToList();
        }

        /// <summary>
        /// Sends a message to a particular user
        /// </summary>
        public void SendMessage(int otherUserId, string message, string clientGuid)
        {
            var myUserId = this.GetMyUserId();
            var myUser = this.db.Users.FirstOrDefault(u => u.Id == myUserId);
            var otherUser = this.db.Users.FirstOrDefault(u => u.Id == otherUserId);

            if (myUser == null || otherUser == null)
                return;

            var dbChatMessage = new Cerebello.Model.ChatMessage()
                {
                    Date = DateTime.UtcNow,
                    Message = message,
                    UserFromId = myUserId,
                    UserToId = otherUserId,
                    PracticeId = myUser.PracticeId
                };

            this.db.ChatMessages.AddObject(dbChatMessage);

            this.db.SaveChanges();

            this.BroadcastMessage(otherUserId, dbChatMessage, clientGuid);
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            var myRoomId = this.GetMyRoomId();
            var myUserId = this.GetMyUserId();

            lock (connections)
            {
                if (!connections.ContainsKey(myRoomId))
                    connections[myRoomId] = new Dictionary<int, List<string>>();

                if (!connections[myRoomId].ContainsKey(myUserId))
                    connections[myRoomId][myUserId] = new List<string>();

                connections[myRoomId][myUserId].Add(this.Context.ConnectionId);
            }

            this.BroadcastUsersList();

            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            var myRoomId = this.GetMyRoomId();
            var myUserId = this.GetMyUserId();

            lock (connections)
            {
                if (connections.ContainsKey(myRoomId))
                    if (connections[myRoomId].ContainsKey(myUserId))
                        if (connections[myRoomId][myUserId].Contains(this.Context.ConnectionId))
                        {
                            connections[myRoomId][myUserId].Remove(this.Context.ConnectionId);
                            if (!connections[myRoomId][myUserId].Any())
                                connections[myRoomId].Remove(myUserId);
                        }
            }

            this.BroadcastUsersList();

            return base.OnDisconnected();
        }
    }
}