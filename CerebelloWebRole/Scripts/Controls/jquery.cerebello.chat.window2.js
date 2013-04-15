﻿(function ($) {

    function ChatWindow(options) {

        // Defaults:
        this.defaults = {
            practice: null,
            myUser: null,
            otherUser: null,
            initialToggleState: "maximized",
            initialFocusState: "focused",
            hub: null,
            onReady: function() { },
            onClose: function () { },
            // triggers when the window changes it's state: minimized or maximized
            onToggleStateChanged: function (currentState) { }
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = null;
        this.chatContainer = null;


        this.addMessage = function (message, clientGuid) {
            var _this = this;
            _this.chatContainer.setToggleState("maximized");

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

            if (message.ClientGuid && $("p[data-val-client-guid='" + message.ClientGuid + "']").length) {
                // in this case, this message is comming from the server AND the current user POSTED the message.
                // so he/she already has this message in the list. We DO NOT need to add the message.
                $("p[data-val-client-guid='" + message.ClientGuid + "']").removeClass("temp-message").removeAttr("data-val-client-guid");
            } else {
                var $messageP = $("<p/>").text(message.Message);
                if (clientGuid)
                    $messageP.attr("data-val-client-guid", clientGuid).addClass("temp-message");
                linkify($messageP);

                // gets the last message to see if it's possible to just append the text
                var $lastMessage = $("div.chat-message:last", _this.chatContainer.$windowInnerContent);
                if ($lastMessage.length && $lastMessage.attr("data-val-user-from") == message.UserFrom.Id) {
                    // we can just append text then
                    $messageP.appendTo($(".chat-text-wrapper", $lastMessage));
                }
                else {
                    // in this case we need to create a whole new message
                    var $chatMessage = $("<div/>").addClass("chat-message").attr("data-val-user-from", message.UserFrom.Id);
                    $chatMessage.appendTo(_this.chatContainer.$windowInnerContent);

                    var $gravatarWrapper = $("<div/>").addClass("chat-gravatar-wrapper").appendTo($chatMessage);
                    var $textWrapper = $("<div/>").addClass("chat-text-wrapper").appendTo($chatMessage);

                    // add text
                    $messageP.appendTo($textWrapper);

                    // add image
                    $("<img/>").attr("src", decodeURI(message.UserFrom.GravatarUrl)).appendTo($gravatarWrapper);
                }

                // scroll to the bottom
                _this.chatContainer.$windowInnerContent.scrollTop(_this.chatContainer.$windowInnerContent[0].scrollHeight);
            }
        };

        this.sendMessage = function (messageText) {
            var _this = this;
            var clientGuid = generateGuid();
            _this.addMessage({
                UserFrom: _this.opts.myUser,
                Message: messageText
            }, clientGuid);

            _this.opts.hub.server.sendMessage(_this.opts.otherUser.Id, messageText, clientGuid);
        };

        this.getToggleState = function () {
            var _this = this;
            return _this.chatContainer.getToggleState();
        };

        this.setVisible = function (value) {
            var _this = this;
            _this.chatContainer.setVisible(value);
        };
    }

    // Separate functionality from object creation
    ChatWindow.prototype = {

        init: function () {
            var _this = this;

            _this.chatContainer = $.chatContainer({
                title: _this.opts.userToName,
                canClose: true,
                initialToggleState: _this.opts.initialToggleState,
                onClose: function (e) {
                    _this.opts.onClose(e);
                },
                onToggleStateChanged: function (toggleState) {
                    _this.opts.onToggleStateChanged(toggleState)
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

            _this.opts.hub.server.getMessageHistory(_this.opts.otherUser.Id).done(function(messageHistory) {
                for (var i = 0; i < messageHistory.length; i++)
                    _this.addMessage(messageHistory[i]);
                
                _this.chatContainer.setVisible(true);

                if (_this.opts.initialFocusState == "focused")
                    _this.chatContainer.$textBox.focus();
                
                // scroll to the bottom
                _this.chatContainer.$windowInnerContent.scrollTop(_this.chatContainer.$windowInnerContent[0].scrollHeight);

                if (_this.opts.onReady)
                    _this.opts.onReady(_this);
            });
        }
    };

    // The actual plugin
    $.chatWindow = function (options) {
        var chatWindow = new ChatWindow(options);
        chatWindow.init();

        return chatWindow;
    };
})(jQuery);