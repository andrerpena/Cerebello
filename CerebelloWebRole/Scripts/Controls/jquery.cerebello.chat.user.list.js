(function ($) {

    // creates a screen to ask the user for deletion confirmation
    function ChatUserList(options) {

        // Defaults:
        this.defaults = {
            userId: null,
            userName: null,
            roomId: null
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = null;

        // there will be one property on this object for each user in the chat
        this.chatWindows = new Object();
        this.lastMessageCheckTimeStamp = null;
        this.chatContainer = null;

        this.createNewChatWindow = function (otherUserId, otherUserName) {
            var _this = this;
            // if this particular chat-window does not exist yet, create it
            var newChatWindow = $.chatWindow({
                roomId: _this.opts.roomId,
                myUserId: _this.opts.userId,
                myUserName: _this.opts.userName,
                otherUserId: otherUserId,
                otherUserName: otherUserName,
                onClose: function () {
                    delete _this.chatWindows[otherUserId];
                }
            });
            _this.chatWindows[otherUserId] = newChatWindow;
        }

        this.getMessages = function () {
            var _this = this;
            $.ajax({
                url: "/chat/getmessages",
                data: {
                    roomId: _this.opts.roomId,
                    myUserId: _this.opts.userId,
                    timestamp: _this.lastMessageCheckTimeStamp
                },
                success: function (data) {
                    _this.lastMessageCheckTimeStamp = data.Timestamp;
                    for (var i = 0; i < data.Messages.length; i++) {
                        if (!data.FromCache) {
                            // in this case this is new message.
                            // we have to FORWARD each of the messages to the destination
                            // window here
                            if (_this.chatWindows[data.Messages[i].UserFrom.Id])
                            // if the chat-window already exists for the given user, updates it.
                                _this.chatWindows[data.Messages[i].UserFrom.Id].addMessage(data.Messages[i].Message);
                            else {
                                _this.createNewChatWindow(data.Messages[i].UserFrom.Id, data.Messages[i].UserFrom.Name);
                            }
                        }
                        // _this.addMessage(data.Messages[i].Message);
                    }
                    _this.getMessages();
                },
                error: function () {
                }
            });
        }

        this.getUserList = function (noWait) {
            var _this = this;

            if (noWait == undefined)
                noWait = false;
            $.ajax({
                url: "/chat/userlist",
                data: {
                    noWait: noWait,
                    roomId: _this.opts.roomId,
                    userId: _this.opts.userId
                },
                success: function (data, s) {

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
                                    _this.createNewChatWindow(data[otherUserId].Id, data[otherUserId].Name);
                            });
                        })(i);
                    }

                    _this.getUserList();
                },

                error: function () {
                    // too bad but we can't let the system down, go ahead and try again
                    // there must be some error logging in the server so, let's not handle this here
                    _this.getUserList();
                }
            });

        }
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

            // when te user leaves this page, he/she should be disconnected
            $(window).unload(
				function () {
				    $.get("/chat/setuseroffline",
					{
					    roomId: _this.opts.roomId,
					    userId: _this.opts.userId
					});
				}
			);

            // first
	    _this.getUserList(true);
	    _this.getMessages();
        }
    };

    // The actual plugin
    $.chatUserList = function (options) {
        var chatUserList = new ChatUserList(options);
        chatUserList.init();
        return chatUserList;
    };
})(jQuery);
