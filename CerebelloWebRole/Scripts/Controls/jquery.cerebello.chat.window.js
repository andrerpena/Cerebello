(function ($) {

    // creates a screen to ask the user for deletion confirmation
    function ChatWindow(options) {

        // Defaults:
        this.defaults = {
            roomId: null,
            myUserId: null,
            myUserName: null,
            otherUserId: null,
            otherUserName: null,
            onClose: function () { }
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = null;
        this.chatContainer = null;

        this.addMessage = function (message) {
            var _this = this;
            $("<div/>").addClass("chat-message").text(message).appendTo(_this.chatContainer.$windowInnerContent);
        };

        this.sendMessage = function (message) {
            var _this = this;
            _this.addMessage(message);
            $.ajax({
                type: "POST",
                url: "/chat/newmessage",
                data: {
                    roomId: _this.opts.roomId,
                    myUserId: _this.opts.myUserId,
                    otherUserId: _this.opts.otherUserId,
                    message: message
                },
                success: function () {
                    // fine
                },
                error: function () {
                    // too bad
                }
            });
        };
    }

    // Separate functionality from object creation
    ChatWindow.prototype = {

        init: function () {
            var _this = this;

            _this.chatContainer = $.chatContainer({
                title: _this.opts.userToName,
                canClose: true,
                onClose: function (e) {
                    _this.opts.onClose(e);
                }
            });

            _this.chatContainer.$textBox.keypress(function (e) {
                if (e.which == 13) {
                    e.preventDefault();
                    if ($(this).val()) {
                        _this.sendMessage($(this).val());
                        $(this).val('');
                    }
                }
            });

            _this.chatContainer.$textBox.focus();
        }
    };

    // The actual plugin
    $.chatWindow = function (options) {
        var chatWindow = new ChatWindow(options);
        chatWindow.init();

        return chatWindow;
    };
})(jQuery);
