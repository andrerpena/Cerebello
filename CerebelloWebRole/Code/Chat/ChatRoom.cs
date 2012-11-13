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
        private const int INACTIVITY_TOLERANCE = 40;

        readonly object usersInRoomLock = new object();
        readonly object messageLock = new object();
        private long lastTimeRoomChanged = DateTime.UtcNow.Ticks;

        public ChatRoom(int id)
        {
            this.Id = id;
            this.Users = new Dictionary<int, ChatUser>();
            this.Messages = new List<ChatMessage>();
        }

        /// <summary>
        /// Total list of users. Include offline
        /// </summary>
        public Dictionary<int, ChatUser> Users { get; private set; }

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

                if (this.Users.ContainsKey(user.Id))
                    throw new Exception("User already existis in the room. User id:" + user.Id);

                this.Users.Add(user.Id, user);
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
                
                this.Users.Remove(user.Id);
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
                return this.Users.ContainsKey(userId);
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
                if (!this.Users.ContainsKey(userId))
                    throw new Exception("User not found in the room. User id:" + userId);

                this.Users[userId].LastActiveOn = DateTime.UtcNow;

                // if this user wasn't online previously, make it online and tell everyone
                if (this.Users[userId].Status == ChatUser.StatusType.Online)
                    return;
                this.Users[userId].Status = ChatUser.StatusType.Online;
                this.NotifyUsersChanged();
            }
        }

        /// <summary>
        /// Removes the given user
        /// </summary>
        public void SetUserOffline(int userId)
        {
            lock (usersInRoomLock)
            {
                if (!this.Users.ContainsKey(userId))
                    throw new Exception("User not found in the room. User id:" + userId);

                var user = this.Users[userId];

                if (user.Status == ChatUser.StatusType.Offline)
                    return;
                user.Status = ChatUser.StatusType.Offline;
                this.NotifyUsersChanged();
            }
        }

        /// <summary>
        /// Returns all users in the room sorted by name.
        /// </summary>
        public List<ChatUser> GetUsers()
        {
            lock (usersInRoomLock)
            {
                var referenceTime = DateTime.UtcNow;
                var inactiveUserIds = from u in this.Users where referenceTime - u.Value.LastActiveOn > TimeSpan.FromSeconds(INACTIVITY_TOLERANCE) select u.Key;

                foreach (var userId in inactiveUserIds)
                    this.Users[userId].Status = ChatUser.StatusType.Offline;

                return (from u in this.Users orderby u.Value.Name select u.Value).ToList();
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
                return this.Messages.Where(m => m.Timestamp > timestamp && m.UserTo.Id == myUserId).ToList();
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
                    Timestamp = DateTime.UtcNow.Ticks,
                    UserFrom = this.Users[userFromId],
                    UserTo = this.Users[userToId]
                };

                this.Messages.Add(newMessage);

                var userFrom = this.Users[userFromId];
                var userTo = this.Users[userToId];

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
        /// <param name="user"></param>
        /// <param name="status"></param>
        private void NotifyUsersChanged()
        {
            this.lastTimeRoomChanged = DateTime.UtcNow.Ticks;
            var usersList = this.GetUsers();
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
    }
}