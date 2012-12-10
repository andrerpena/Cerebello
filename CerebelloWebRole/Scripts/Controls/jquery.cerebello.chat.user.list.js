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

        this.createNewChatWindow = function (otherUser, initialToggleState) {
            var _this = this;

            if (!initialToggleState)
                initialToggleState = "maximized";

            // if this particular chat-window does not exist yet, create it
            var newChatWindow = $.chatWindow({
                practice: _this.opts.practice,
                myUser: _this.opts.user,
                otherUser: otherUser,
                initialToggleState: initialToggleState,
                onClose: function () {
                    delete _this.chatWindows[otherUser.Id];
                    _this.saveWindows();
                },
                onToggleStateChanged: function (toggleState) {
                    _this.saveWindows();
                }
            });

            _this.chatWindows[otherUser.Id.toString()] = newChatWindow;
            _this.saveWindows();
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

            _this.chatContainer.setVisible(true);
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
                    var initialToggleState = openedChatWindows[i].toggleState;
                    var user = null;
                    $.ajax({
                        type: "GET",
                        async: false,
                        url: "/p/" + _this.opts.practice + "/chat/getuserinfo",
                        data: {
                            userId: otherUserId
                        },
                        cache: false,
                        success: function (data) {
                            try {
                                user = data.User;
                            } catch (ex) {
                                user = null;
                            }
                        },
                        error: function () {
                            user = null;
                        }
                    });
                    if (user) {
                        if (!_this.chatWindows[otherUserId])
                            _this.createNewChatWindow(user, initialToggleState);
                    } else {
                        // when an error occur, the state of this cookie invalid
                        // it must be destroyed
                        eraseCookie("chat_state");
                    }
                }
            }
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

            $.addLongPollingListener("chat",
                function (event) {
                    // success
                    if (event.EventKey == "new-messages")
                        _this.processMessagesAjaxResult(event.Data);
                    else if (event.EventKey == "user-list")
                        _this.processUserListAjaxResult(event.Data);
                },
                function (e) {
                    var errorMessage;
                    switch (e.status) {
                        case 403:
                            errorMessage = "Seu usuário não está logado ou não possui permissão para acessar o bate-papo no momento.";
                            _this.chatContainer.getContent().html($("<div/>").addClass("message-warning").text(errorMessage).appendTo(_this.chatContainer.getContent()));
                            break;
                        case 500:
                            errorMessage = "Ocorreu um erro ao tentar carregar o bate-papo.";
                            _this.chatContainer.getContent().html($("<div/>").addClass("message-warning").text(errorMessage).appendTo(_this.chatContainer.getContent()));
                            break;
                        default:
                            // chances are that the user just clicked a link. When you click a link
                            // the pending ajaxes break and we'll just hide the window
                            _this.chatContainer.setVisible(false);
                    }
                }
            );

            _this.loadWindows();
        }
    };

    // The actual plugin
    $.chatUserList = function (options) {
        var chatUserList = new ChatUserList(options);
        chatUserList.init();
        return chatUserList;
    };
})(jQuery);
