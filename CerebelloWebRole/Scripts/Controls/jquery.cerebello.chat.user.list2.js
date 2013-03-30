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

        this.createNewChatWindow = function (otherUser, initialToggleState, initialFocusState) {
            var _this = this;

            if (!initialToggleState)
                initialToggleState = "maximized";

            if (!initialFocusState)
                initialFocusState = "focused";

            // if this particular chat-window does not exist yet, create it
            var newChatWindow = $.chatWindow({
                practice: _this.opts.practice,
                myUser: _this.opts.user,
                otherUser: otherUser,
                newMessageUrl: _this.opts.newMessageUrl,
                messageHistoryUrl: _this.opts.messageHistoryUrl,
                initialToggleState: initialToggleState,
                initialFocusState: initialFocusState,
                hub: _this.hub,
                onClose: function () {
                    delete _this.chatWindows[otherUser.Id];
                    $.organizeChatContainers();
                    _this.saveWindows();
                },
                onToggleStateChanged: function (toggleState) {
                    _this.saveWindows();
                }
            });
            
            // this cannot be in t
            _this.chatWindows[otherUser.Id.toString()] = newChatWindow;
            _this.saveWindows();
        };

        this.processUserListAjaxResult = function (data) {
            /// <summary>Handles the list of users coming from the server</summary>
            /// <param name="data" type="Array">List of users</param>
            var _this = this;
            _this.chatContainer.getContent().html('');
            if (data.length == 1) {
                $("<div/>").addClass("user-list-empty").text("Não existem outros usuários").appendTo(_this.chatContainer.getContent());
            }
            else {
                for (var i = 0; i < data.length; i++) {
                    if (data[i].Id != _this.opts.user.Id) {
                        var $user = $("<div/>")
                        .addClass("user-list-item")
                        .attr("data-val-id", data[i].Id)
                        .text(data[i].Name)
                        .appendTo(_this.chatContainer.getContent());

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
                                } else
                                    _this.createNewChatWindow(data[otherUserId]);
                            });
                        })(i);
                    }
                }
            }

            _this.chatContainer.setVisible(true);
        };

        this.saveWindows = function () {
            var _this = this;
            var openedChatWindows = new Array();
            for (var otherUserId in _this.chatWindows) {
                openedChatWindows.push({
                    userId: otherUserId,
                    toggleState: _this.chatWindows[otherUserId].getToggleState()
                });
            }
            createCookie("chat_state", JSON.stringify(openedChatWindows), 365);
        };

        this.loadWindows = function () {
            var _this = this;
            var cookie = readCookie("chat_state");
            if (cookie) {
                var openedChatWindows = JSON.parse(cookie);
                for (var i = 0; i < openedChatWindows.length; i++) {
                    var otherUserId = openedChatWindows[i].userId;
                    _this.hub.server.getUserInfo(otherUserId).done(function (user) {
                        if (user) {
                            if (!_this.chatWindows[otherUserId])
                                _this.createNewChatWindow(user, null, "blured");
                        } else {
                            // when an error occur, the state of this cookie invalid
                            // it must be destroyed
                            eraseCookie("chat_state");
                        }
                    });
                }
            }
        };

        this.playSound = function (filename) {
            /// <summary>Plays a notification sound</summary>
            /// <param name="filename" type="String">The file path without extension</param>
            var $soundContainer = $("#soundContainer");
            if (!$soundContainer.length)
                $soundContainer = $("<div>").attr("id", "soundContainer").appendTo($("body"));
            $soundContainer.html('<audio autoplay="autoplay"><source src="' + filename + '.mp3" type="audio/mpeg" /><source src="' + filename + '.ogg" type="audio/ogg" /><embed hidden="true" autostart="true" loop="false" src="' + filename + '.mp3" /></audio>');
        };
    }

    // Separate functionality from object creation
    ChatUserList.prototype = {

        init: function () {
            var _this = this;

            var mainChatWindowChatState = readCookie("main_window_chat_state");
            if (!mainChatWindowChatState)
                mainChatWindowChatState = "maximized";

            _this.chatContainer = $.chatContainer({
                title: "Bate-papo",
                showTextBox: false,
                canClose: false,
                initialToggleState: mainChatWindowChatState,
                onToggleStateChanged: function (toggleState) {
                    createCookie("main_window_chat_state", toggleState);
                }
            });

            _this.hub = $.connection.chatHub;
            _this.hub.client.newMessage = function (message) {


                if (message.UserFrom.Id != _this.opts.user.Id) {
                    // in this case this message did not came from myself
                    if (!_this.chatWindows[message.UserFrom.Id])
                        _this.createNewChatWindow(message.UserFrom);
                    else
                        _this.chatWindows[message.UserFrom.Id].addMessage(message);

                    _this.playSound("/content/sounds/chat");

                    // play sound here
                } else {
                    if (_this.chatWindows[message.UserTo.Id]) {
                        _this.chatWindows[message.UserTo.Id].addMessage(message);
                    }
                }
            };

            _this.hub.client.usersListChanged = function (usersList) {
                _this.processUserListAjaxResult(usersList);
            };

            if (!window.hubReady)
                window.hubReady = $.connection.hub.start();

            window.hubReady.done(function () {
                _this.loadWindows();
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
