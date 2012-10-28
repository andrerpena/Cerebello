(function ($) {

    // creates a chat user-list
    function ChatUserList(options) {

        // Defaults:
        this.defaults = {
            user: null,
            practice: null
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = null;

        // there will be one property on this object for each user in the chat
        // the property name is the other user id (toStringed)
        this.chatWindows = new Object();
        this.lastMessageCheckTimeStamp = null;
        this.chatContainer = null;

        this.createNewChatWindow = function (otherUser) {
            var _this = this;
            // if this particular chat-window does not exist yet, create it
            var newChatWindow = $.chatWindow({
                practice: _this.opts.practice,
                myUser: _this.opts.user,
                otherUser: otherUser,
                onClose: function () {
                    delete _this.chatWindows[otherUser.Id];
                }
            });
            _this.chatWindows[otherUser.Id.toString()] = newChatWindow;
        };

        this.processUserListAjaxResult = function (data) {
            /// <summary>Handles the list of users coming from the server</summary>
            /// <param name="data" type="Array">List of users</param>
            var _this = this;
            _this.chatContainer.getContent().html('');
            for (var i = 0; i < data.length; i++) {
                var $user = $("<div/>").addClass("user-list-item").attr("data-val-id", data[i].Id).text(data[i].Name).appendTo(_this.chatContainer.getContent());
                if (data[i].Status == 0)
                    $user.addClass("offline");
                else
                    $user.addClass("online");

                // I must clusure the 'i'
                (function (otherUserId) {
                    // handles clicking in a user. Starts up a new chat session
                    $user.click(function () {
                        if (_this.chatWindows[data[otherUserId].Id]) {
                            // focus chat-window
                        }
                        else
                            _this.createNewChatWindow(data[otherUserId]);
                    });
                })(i);
            }
        };

        this.processMessagesAjaxResult = function (data) {
            /// <summary>Handles the list of messages coming from the server</summary>
            /// <param name="data" type="Object">Messages</param>
            // this otherUserId is a number toStringed
            var _this = this;
            for (var otherUserId in data) {

                // here there's something tricky.
                // if the current user does not have a window opened relative to the user that just sent the message, we need 
                // to load the history for that user, meaning we will have to return to the server.
                // Therefore, it's a little bit easier just to ignore this message and get the WHOLE HISTORY in the server now.
                if (!_this.chatWindows[otherUserId])
                    _this.createNewChatWindow(data[otherUserId][0].UserFrom);

                else {
                    for (var i = 0; i < data[otherUserId].length; i++)
                        _this.chatWindows[otherUserId].addMessage(data[otherUserId][i]);
                }
            }
        };
    }

    // Separate functionality from object creation
    ChatUserList.prototype = {

        init: function () {
            var _this = this;

            _this.chatContainer = $.chatContainer({
                title: "Bate-papo",
                showTextBox: false,
                canClose: false
            });

            $.addLongPollingListener("chat", function (event) {
                // success
                if (event.EventKey == "new-messages")
                    _this.processMessagesAjaxResult(event.Data);
                else if (event.EventKey == "user-list")
                    _this.processUserListAjaxResult(event.Data);
            },
            function () {
                // error
            });
        }
    };

    // The actual plugin
    $.chatUserList = function (options) {
        var chatUserList = new ChatUserList(options);
        chatUserList.init();
        return chatUserList;
    };
})(jQuery);
