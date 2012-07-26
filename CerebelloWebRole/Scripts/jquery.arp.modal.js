(function ($) {

    //permite adicionar uma marca d'água nos input[type='text']
    function Modal(options) {

        //Defaults:
        this.defaults = {
            url: "",
            title: "",
            data: {},
            ok: function () { },
            cancel: function () { },
            width: 400,
            height: 300
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        this.$mask = null;
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
            _this.$el = $("<div class='modal'></div>").appendTo(document.body).css({
                top: $(window).height() / 2 - _this.opts.height / 2,
                left: $(window).width() / 2 - _this.opts.width / 2,
                width: _this.opts.width,
                'min-height': _this.opts.height,
                'z-index': window._modalZIndex++
            });

            var $modalHeader = $("<div class='modal-header'><div class='modal-close'></div><div class='modal-title'>" + _this.opts.title + "</div><div style='clear:both'></div></div>").appendTo(this.$el);
            $(".modal-close", $modalHeader).click(function (e) {
                e.preventDefault();
                _this.close();
            });

            var modalContent = $("<div class='content'></div>").appendTo(this.$el);

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

            $.ajax({
                url: _this.opts.url,
                data: _this.opts.data,
                success: function (html) {
                    _this.$el.show();
                    modalContent.html(html);
                    _this.resize();
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