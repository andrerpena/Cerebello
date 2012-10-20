(function ($) {
    // creates a screen to ask the user for deletion confirmation
    function ChatContainer(options) {

        // Defaults:
        this.defaults = {
            objectType: null,
            objectName: null,
            title: null,
            canClose: true,
            showTextBox: true,
            onClose: function (chatContainer) {  }
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = null;
        this.$window = null;
        this.$windowTitle = null;
        this.$windowInnerContent = null;
        this.$textBox = null;
    }

    // Separate functionality from object creation
    ChatContainer.prototype = {

        init: function () {
            var _this = this;

            // container
            _this.$window = $("<div/>").addClass("chat-window").appendTo($("body"));

            // title
            _this.$windowTitle = $("<div/>").addClass("chat-window-title").appendTo(_this.$window);
            if (_this.opts.canClose) {
                var $closeButton = $("<div/>").addClass("close").appendTo(_this.$windowTitle);
                $closeButton.click(function (e) {
                    e.stopPropagation();

                    // removes this item from the collection
                    for (var i = 0; i < $._chatContainers.length; i++) {
                        if ($._chatContainers[i] == _this) {
                            $._chatContainers.splice(i, 1);
                            break;
                        }
                    }

                    // removes the window
                    _this.$window.remove();

                    // triggers the event
                    _this.opts.onClose(_this);
                });

            }
            $("<div/>").addClass("text").text(_this.opts.title).appendTo(_this.$windowTitle);


            // content
            var $windowContent = $("<div/>").addClass("chat-window-content").appendTo(_this.$window);
            _this.$windowInnerContent = $("<div/>").addClass("chat-window-inner-content").appendTo($windowContent);

            // text-box-wrapper
            if (_this.opts.showTextBox) {
                var $windowTextBoxWrapper = $("<div/>").addClass("chat-window-text-box-wrapper").appendTo($windowContent);
                _this.$textBox = $("<input/>").attr("type", "text").addClass("chat-window-text-box").appendTo($windowTextBoxWrapper);
            }

            // wire everything up
            _this.$windowTitle.click(function () {
                $windowContent.toggle();
                if ($windowContent.is(":visible") && _this.opts.showTextBox)
                    _this.$textBox.focus();
            });

            // enlists this container in the containers
            if (!$._chatContainers)
                $._chatContainers = new Array();
            $._chatContainers.push(_this);

            $.organizeChatContainers();
        },

        getContent: function () {
            var _this = this;
            return _this.$windowInnerContent;
        },

        setTitle: function (title) {
            var _this = this;
            $("div[class=text]", _this.$windowTitle).text(title);
        }

    };

    // The actual plugin
    $.chatContainer = function (options) {
        var chatContainer = new ChatContainer(options);
        chatContainer.init();

        return chatContainer;
    };

    $.organizeChatContainers = function () {
        // this is the initial right offset
        var rightOffset = 30;
        var deltaOffset = 20;
        for (var i = 0; i < $._chatContainers.length; i++) {
            $._chatContainers[i].$window.css("right", rightOffset);
            rightOffset += $._chatContainers[i].$window.outerWidth() + deltaOffset;
        }
    };

})(jQuery);

