using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.Chat
{
    /// <summary>
    /// A Chat room. Chat room users are STATIC, meaning they don't change, unless the practice adds another user. A user doesn't join
    /// a room. It's in a room automatically. The only thing that changes is the status.
    /// </summary>
    public class ChatRoom
    {
        /// <summary>
        /// Time after which the user is considered inactive and elegible for 
        /// removal (in seconds)
        /// </summary>
        private static readonly TimeSpan inactivityTolerance = TimeSpan.FromSeconds(40);

        readonly object usersInRoomLock = new object();
        readonly object messageLock = new object();
        private long lastTimeRoomChanged = GetUtcNow().Ticks;

        private static DateTime GetUtcNow()
        {
            return DateTime.UtcNow + DebugConfig.CurrentTimeOffset;
        }

        public ChatRoom(int id)
        {
            this.Id = id;
            this.UsersById = new Dictionary<int, ChatUser>();
            this.Messages = new List<ChatMessage>();
        }

        /// <summary>
        /// Total list of users. Include offline
        /// </summary>
        public Dictionary<int, ChatUser> UsersById { get; private set; }

        /// <summary>
        /// All the messages in the room, from all users.
        /// </summary>
        /// <remarks>
        /// This data structure is definitely not the better one for this purpose.
        /// ToDo: fix this
        /// </remarks>
        public List<ChatMessage> Messages { get; private set; }

        /// <summary>
        /// Chat room Id. 
        /// This Id corresponds to the Practice Id
        /// </summary>
        private int Id { get; set; }

        /// <summary>
        /// Adds a user to the room
        /// </summary>
        public void AddUser(ChatUser user)
        {
            lock (usersInRoomLock)
            {
                if (user == null) throw new ArgumentNullException("user");

                if (this.UsersById.ContainsKey(user.Id))
                    throw new Exception("User already existis in the room. User id:" + user.Id);

                this.UsersById.Add(user.Id, user);
                // I'm infering he/she is online now by the usage of this method. I'm not sure this will work
                this.NotifyUsersChanged();
            }
        }

        /// <summary>
        /// Removes a user from the room
        /// </summary>
        public void RemoveUser(ChatUser user)
        {
            lock (usersInRoomLock)
            {
                if (user == null) throw new ArgumentNullException("user");

                this.UsersById.Remove(user.Id);
                this.NotifyUsersChanged();
            }
        }

        /// <summary>
        /// Returns whether or not the current user exists
        /// </summary>
        /// <param name="userId"></param>
        public bool UserExists(int userId)
        {
            lock (usersInRoomLock)
            {
                return this.UsersById.ContainsKey(userId);
            }
        }

        /// <summary>
        /// Adds a user to the room. If the user is in the room already, updates his/her
        /// LastActiveOn
        /// </summary>
        public void SetUserOnline(int userId)
        {
            lock (usersInRoomLock)
            {
                if (!this.UsersById.ContainsKey(userId))
                    throw new Exception("User not found in the room. User id:" + userId);

                this.UsersById[userId].LastActiveOn = GetUtcNow();

                // if this user wasn't online previously, make it online and tell everyone
                if (this.UsersById[userId].Status != ChatUser.StatusType.Online)
                {
                    this.UsersById[userId].Status = ChatUser.StatusType.Online;
                    this.NotifyUsersChanged();
                }
            }
        }

        /// <summary>
        /// Removes the given user
        /// </summary>
        public void SetUserOffline(int userId)
        {
            lock (usersInRoomLock)
            {
                if (!this.UsersById.ContainsKey(userId))
                    throw new Exception("User not found in the room. User id:" + userId);

                var user = this.UsersById[userId];

                if (user.Status == ChatUser.StatusType.Offline)
                    return;
                user.Status = ChatUser.StatusType.Offline;
                this.NotifyUsersChanged();
            }
        }

        /// <summary>
        /// Returns all users in the room sorted by name.
        /// This will also update the status of the unseen users to offline.
        /// </summary>
        public List<ChatUser> GetUsersAndUpdateStatus()
        {
            lock (usersInRoomLock)
            {
                var lastSeenLimit = GetUtcNow() - inactivityTolerance;
                var inactiveUsers = this.UsersById.Values.Where(u => u.LastActiveOn < lastSeenLimit);

                foreach (var user in inactiveUsers)
                    user.Status = ChatUser.StatusType.Offline;

                return this.UsersById.Values.OrderBy(u => u.Name).ToList();
            }
        }

        /// <summary>
        /// Returns the messages related to the given user.
        /// </summary>
        /// <param name="myUserId"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public List<ChatMessage> GetMessagesTo(int myUserId, long timestamp)
        {
            lock (messageLock)
            {
                var nowTimeStamp = GetUtcNow().Ticks;
                return this.Messages.Where(m => m.Timestamp > timestamp && m.Timestamp <= nowTimeStamp && m.UserTo.Id == myUserId).ToList();
            }
        }

        /// <summary>
        /// Returns all the messages between the two users given, that occurred PRIOR to the passed timeStamp.
        /// This list is INVERTED to make things easier in the client
        /// </summary>
        /// <param name="myUserId"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public List<ChatMessage> GetMessagesBetween(int myUserId, int otherUserId, long? timestamp = null)
        {
            lock (messageLock)
            {
                var query = this.Messages.Where(m => (m.UserTo.Id == myUserId && m.UserFrom.Id == otherUserId) || (m.UserTo.Id == otherUserId && m.UserFrom.Id == myUserId));
                if (timestamp.HasValue)
                    query = query.Where(m => m.Timestamp < timestamp);
                return query.OrderBy(m => m.Timestamp).ToList();
            }
        }

        public void AddMessage(int userFromId, int userToId, string message)
        {
            lock (messageLock)
            {
                var newMessage = new ChatMessage()
                {
                    Message = message,
                    Timestamp = GetUtcNow().Ticks,
                    UserFrom = this.UsersById[userFromId],
                    UserTo = this.UsersById[userToId]
                };

                this.Messages.Add(newMessage);

                var userFrom = this.UsersById[userFromId];
                var userTo = this.UsersById[userToId];

                this.NotifyNewMessage(userFrom, userTo, newMessage);
            }
        }

        /// <summary>
        /// Event that is triggered when a room changed
        /// </summary>
        public event Action<int, List<ChatUser>> UserStatusChanged;

        /// <summary>
        /// This method is supposed to be used inside a 'using' clause.
        /// </summary>
        /// <param name="onUserStatusChanged"></param>
        /// <returns></returns>
        public ChatRoomUsersSubscription SubscribeForUsersChange(Action<int, List<ChatUser>> onUserStatusChanged)
        {
            if (onUserStatusChanged == null) throw new ArgumentNullException("onUserStatusChanged");
            return new ChatRoomUsersSubscription(this, onUserStatusChanged);
        }

        /// <summary>
        /// Notifies subscribers
        /// </summary>
        private void NotifyUsersChanged()
        {
            this.lastTimeRoomChanged = GetUtcNow().Ticks;
            var usersList = this.GetUsersAndUpdateStatus();
            if (this.UserStatusChanged != null)
                this.UserStatusChanged(this.Id, usersList);
        }

        /// <summary>
        /// Event that is triggered when a room changed
        /// </summary>
        public event Action<int, ChatUser, ChatUser, ChatMessage> MessagesChanged;

        /// <summary>
        /// This method is supposed to be used inside a 'using' clause.
        /// </summary>
        /// <param name="onNewMessage"></param>
        /// <returns></returns>
        public ChatRoomMessagesSubscription SubscribeForMessagesChange(Action<int, ChatUser, ChatUser, ChatMessage> onNewMessage)
        {
            if (onNewMessage == null) throw new ArgumentNullException("onNewMessage");
            return new ChatRoomMessagesSubscription(this, onNewMessage);
        }

        /// <summary>
        /// Notifies subscribers that a new Message has arrived
        /// </summary>
        /// <param name="userFrom"></param>
        /// <param name="userTo"></param>
        private void NotifyNewMessage(ChatUser userFrom, ChatUser userTo, ChatMessage message)
        {
            if (this.MessagesChanged != null)
                this.MessagesChanged(this.Id, userFrom, userTo, message);
        }

        /// <summary>
        /// Indicates whether or not the users in this room changed since the given timestamp
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public bool HasChangedSince(long timestamp)
        {
            return this.lastTimeRoomChanged > timestamp;
        }

        internal void CheckForUnseenUsers()
        {
            // checking for long unseen users, that still have online status
            var lastSeenLimit = GetUtcNow() - inactivityTolerance;
            bool hasUnseenUsers = this.UsersById.Values.Any(u => u.LastActiveOn < lastSeenLimit && u.Status != ChatUser.StatusType.Offline);

            if (hasUnseenUsers)
                this.NotifyUsersChanged();
        }
    }
}