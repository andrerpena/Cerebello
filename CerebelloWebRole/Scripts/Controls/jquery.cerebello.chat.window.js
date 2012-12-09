(function ($) {

    function ChatWindow(options) {

        // Defaults:
        this.defaults = {
            practice: null,
            myUser: null,
            otherUser: null,
            onClose: function () { }
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = null;
        this.chatContainer = null;


        this.addMessage = function (message) {
            var _this = this;
            
            // takes a jQuery element and replace it's content that seems like an URL with an
            // actual link or e-mail
            function linkify($element) {
                var inputText = $element.html();
                var replacedText, replacePattern1, replacePattern2, replacePattern3;

                //URLs starting with http://, https://, or ftp://
                replacePattern1 = /(\b(https?|ftp):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/gim;
                replacedText = inputText.replace(replacePattern1, '<a href="$1" target="_blank">$1</a>');

                //URLs starting with "www." (without // before it, or it'd re-link the ones done above).
                replacePattern2 = /(^|[^\/])(www\.[\S]+(\b|$))/gim;
                replacedText = replacedText.replace(replacePattern2, '$1<a href="http://$2" target="_blank">$2</a>');

                //Change email addresses to mailto:: links.
                replacePattern3 = /(\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,6})/gim;
                replacedText = replacedText.replace(replacePattern3, '<a href="mailto:$1">$1</a>');

                return $element.html(replacedText);
            }

            // gets the last message to see if it's possible to just append the text
            var $lastMessage = $("div.chat-message:last", _this.chatContainer.$windowInnerContent);
            if ($lastMessage.length && $lastMessage.attr("data-val-user-from") == message.UserFrom.Id) {
                // we can just append text then
                linkify($("<p/>").text(message.Message)).appendTo($(".chat-text-wrapper", $lastMessage));
            }
            else {
                // in this case we need to create a whole new message
                var $chatMessage = $("<div/>").addClass("chat-message").attr("data-val-user-from", message.UserFrom.Id);
                $chatMessage.appendTo(_this.chatContainer.$windowInnerContent);

                var $gravatarWrapper = $("<div/>").addClass("chat-gravatar-wrapper").appendTo($chatMessage);
                var $textWrapper = $("<div/>").addClass("chat-text-wrapper").appendTo($chatMessage);

                // add text
                linkify($("<p/>").text(message.Message)).appendTo($textWrapper);
                

                // add image
                $("<img/>").attr("src", decodeURI(message.UserFrom.GravatarUrl)).appendTo($gravatarWrapper);
            }

            // scroll to the bottom
            _this.chatContainer.$windowInnerContent.scrollTop(_this.chatContainer.$windowInnerContent[0].scrollHeight);
        };

        this.sendMessage = function (messageText) {
            var _this = this;

            _this.addMessage({
                UserFrom: _this.opts.myUser,
                Message: messageText
            });
            $.ajax({
                type: "POST",
                url: "/p/" + _this.opts.practice + "/chat/newmessage",
                data: {
                    otherUserId: _this.opts.otherUser.Id,
                    message: messageText
                },
                cache: false,
                success: function () {
                    // fine
                },
                error: function () {
                    // too bad
                }
            });
        };

        this.loadHistory = function () {
            var _this = this;

            $.ajax({
                type: "GET",
                async: false,
                url: "/p/" + _this.opts.practice + "/chat/getmessagehistory",
                data: {
                    otherUserId: _this.opts.otherUser.Id
                },
                cache: false,
                success: function (data) {
                    // fine
                    // this otherUserId is a number toStringed
                    for (var otherUserId in data.Messages) {
                        for (var i = 0; i < data.Messages[otherUserId].length; i++)
                            _this.addMessage(data.Messages[otherUserId][i], true);
                    }
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

            _this.chatContainer.setTitle(_this.opts.otherUser.Name);
            _this.chatContainer.$textBox.focus();

            this.loadHistory();
        }
    };

    // The actual plugin
    $.chatWindow = function (options) {
        var chatWindow = new ChatWindow(options);
        chatWindow.init();

        return chatWindow;
    };
})(jQuery);
