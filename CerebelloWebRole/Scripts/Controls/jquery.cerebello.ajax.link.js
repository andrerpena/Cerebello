(function ($) {

    function AjaxLink(el, options) {

        //Defaults:
        this.defaults = {
            url: "",
            data: {},
            dataType: "json", // json is the default for this app
            success: function () { },
            error: function () { }
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = $(el);
    }

    AjaxLink.prototype = {

        init: function () {
            var _this = this;
            _this.$el.click(function () {
                _this.$el.addClass("loading");
                $.ajax({
                    url: _this.opts.url,
                    data: $.isFunction(_this.opts.data) ? _this.opts.data() : _this.opts.data,
                    dataType: _this.opts.dataType,
                    success: function (data, status, e) {
                        _this.opts.success.call(_this.$el, data);
                        _this.$el.removeClass("loading");
                    },
                    error: function () {
                        _this.opts.error.call(_this.$el);
                        _this.$el.removeClass("loading");
                    }
                });
            });
        }
    };

    $.fn.ajaxLink = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new AjaxLink(this, options);
                rev.init();
                $(this).data('ajaxLink', rev);
            });
        }
        return this;
    };

})(jQuery);