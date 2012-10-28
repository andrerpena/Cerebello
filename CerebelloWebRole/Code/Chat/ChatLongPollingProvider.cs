using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Cerebello.Model;
using CerebelloWebRole.Code.LongPolling;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Chat
{
    public class ChatLongPollingProvider : LongPollingProvider
    {
        /// <summary>
        /// Returns a LongPollingEvent relative to the new messages.
        /// This LongPollingEvent will be sent up to the client
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private LongPollingEvent GetLongPollingEventForMessages([NotNull] IEnumerable<ChatMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException("messages");
            return new LongPollingEvent()
                {
                    Data =
                        messages.GroupBy(cm => cm.UserFrom.Id).ToDictionary(g => g.Key.ToString(), g => g.ToList()),
                    EventKey = "new-messages",
                    ProviderName = "chat"
                };
        }

        /// <summary>
        /// Returns a LongPollingEvent relative to the given users-list
        /// This LongPollingEvent will be sent up to the client
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        private LongPollingEvent GetLongPollingEventForRoomUsersChanged([NotNull] IEnumerable<ChatUser> users, int myUserId)
        {
            if (users == null) throw new ArgumentNullException("users");
            var roomUsersExcludingCurrentUser = users.Where(u => u.Id != myUserId).OrderBy(u => u.Name).ToList();
            return new LongPollingEvent()
                {
                    Data = roomUsersExcludingCurrentUser,
                    EventKey = "user-list",
                    ProviderName = "chat"
                };
        }

        public override IEnumerable<LongPollingEvent> WaitForEvents(int userId, int practiceId, long timestamp, [NotNull] CerebelloEntities db)
        {
            if (db == null) throw new ArgumentNullException("db");

            var chatRoom = ChatServerHelper.SetupRoomIfNonexisting(db, practiceId);
            ChatServerHelper.SetupUserIfNonexisting(db, practiceId, userId);
            chatRoom.SetUserOnline(userId);


            var events = new List<LongPollingEvent>();

            // First, see if there are existing messages, in this case we're gonna
            // just return them
            var existingMessages = ChatServer.Rooms[practiceId].GetMessagesTo(userId, timestamp);
            // the reason why this "timestamp > 0" is that timestamp is 0 when it's the first time user
            // user is requesting messages. But I don't want to send old messages to the user as if they were
            // new
            if (existingMessages.Any() && timestamp > 0)
                events.Add(this.GetLongPollingEventForMessages(existingMessages));

            // now, see if the users list changed since last time
            if (ChatServer.Rooms[practiceId].HasChangedSince(timestamp))
                events.Add(GetLongPollingEventForRoomUsersChanged(ChatServer.Rooms[practiceId].GetUsers(), userId));

            if (!events.Any())
            {
                // In this case, nothing actually changed since last time, so we're gonna have
                // to wait

                var wait = new AutoResetEvent(false);

                var roomChangeSubscription =
                    ChatServer.Rooms[practiceId].SubscribeForUsersChange((r, users) =>
                        {
                            events.Add(this.GetLongPollingEventForRoomUsersChanged(users, userId));
                            wait.Set();
                        });

                var messagesChangeSubscription =
                    ChatServer.Rooms[practiceId].SubscribeForMessagesChange((r, userFrom, userTo, message) =>
                        {
                            // all messages destinated to the given user must be considered
                            if (userTo.Id != userId)
                                return;
                            events.Add(this.GetLongPollingEventForMessages(new List<ChatMessage>() { message }));
                            wait.Set();
                        });

                // will STOP the thread here
                wait.WaitOne(LongPollingProvider.WAIT_TIMEOUT);

                roomChangeSubscription.Dispose();
                messagesChangeSubscription.Dispose();
            }

            return events;
        }
    }
}