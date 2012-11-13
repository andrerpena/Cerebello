(function ($) {

    //permite adicionar uma marca d'água nos input[type='text']
    function Modal(options) {

        //Defaults:
        this.defaults = {
            // url that will return the modal's content
            url: "",
            // this function allows for manually determining the content of the modal.
            // this should be set as opposed to 'url' (one or another)
            buildContent: null,
            title: "",
            data: {},
            ok: function () { },
            cancel: function () { },
            width: 400,
            // you can set height to null if you want auto-height
            height: 300
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        this.$mask = null;

        // this is gonna be initialized further
        this.$el = null;

        this.resize = function () {

            this.$el.css({
                top: $(window).height() / 2 - this.$el.height() / 2,
                left: $(window).width() / 2 - this.$el.width() / 2
            });

            this.$mask.css({
                width: $(window).width(),
                height: $(document).height()
            });
        }
    }

    // Separate functionality from object creation
    Modal.prototype = {

        init: function () {
            var _this = this;

            if (!window._modalZIndex)
                window._modalZIndex = 90000;

            // adiciono a máscara
            _this.$mask = $("<div class='modal-mask'></div>").appendTo(document.body).css({
                width: $(window).width(),
                height: $(document).height(),
                'z-index': window._modalZIndex++
            });

            _this.$mask.fadeIn("fast");

            // adiciono o popup em si
            _this.$el = $("<div />").addClass("modal").appendTo(document.body).css({
                'z-index': window._modalZIndex++
            });

            // if width has been specified, sets it
            if (_this.opts.width)
                _this.$el.css("max-width", _this.opts.width);

            // if height has been specified, sets it
            if (_this.opts.height)
                _this.$el.css("min-height", _this.opts.height);

            var $modalHeader = $("<div class='modal-header'><div class='modal-close'></div><div class='modal-title'>" + _this.opts.title + "</div><div style='clear:both'></div></div>").appendTo(this.$el);
            $(".modal-close", $modalHeader).click(function (e) {
                e.preventDefault();
                _this.close();
            });

            // this "spare" div that seems to have no function inside the content is an "error preventer" div
            // when you use a $.modalForm inside the $.modal, it will REPLACE the "form" parent element with whatever comes from the server.
            // this extra <div/> represents something that can be replaced without causing damage
            var $content = $("<div class='content'><div></div><div class='loading'></div></div>").appendTo(this.$el);

            this.$el.bind("modal-ok", function (e, data) {
                _this.opts.ok(data);
                _this.close();
            });

            this.$el.bind("modal-cancel", function (e, data) {
                _this.opts.cancel();
                _this.close();
            });

            this.$el.bind("modal-resize", function (e, data) {
                _this.resize();
            });

            if (_this.opts.buildContent) {
                _this.$el.show();
                _this.opts.buildContent($content);
                _this.resize();
            }
            else
                $.ajax({
                    url: _this.opts.url,
                    data: _this.opts.data,
                    success: function (html) {
                        _this.$el.show();
                        $content.html(html);
                        _this.resize();
                        $("input:text, textarea, select", $content).first().focus();
                    },
                    error: function() {
                        _this.close();
                        throw "There was an error fetching the modal content";
                    }
                });

            var resizeTimer;
            $(window).resize(function () {
                clearTimeout(resizeTimer);
                resizeTimer = setTimeout(function () {
                    _this.resize();
                }, 100);
            });

        },

        close: function () {
            var _this = this;

            _this.$mask.fadeOut("fast", function () {
                _this.$mask.remove();
            });

            _this.$el.fadeOut("fast", function () {
                _this.$el.remove();
            });
        }
    }

    $.modal = function (options) {
        var modal = new Modal(options);
        modal.init();

        return modal;
    };

})(jQuery);