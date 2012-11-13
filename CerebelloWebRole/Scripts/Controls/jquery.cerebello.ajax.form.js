(function ($) {

    function AjaxForm(el, options) {

        //Defaults:
        this.defaults = {
            success: function () { },
            error: function () { }
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = $(el);
    }

    AjaxForm.prototype = {

        init: function () {
            var _this = this;
            $.validator.unobtrusive.parse(_this.$el);
            _this.$el.submit(function () {
                if ($(this).valid()) {
                    $.ajax({
                        url: this.action,
                        type: this.method,
                        data: $(this).serialize(),
                        success: function (data, status, e) {
                            var contentTypeHeader = e.getResponseHeader("Content-Type");
                            var contentType;
                            if (contentTypeHeader.indexOf("text/html") != -1)
                                contentType = "html";
                            else if (contentTypeHeader.indexOf("application/json") != -1)
                                contentType = "json";
                            _this.opts.success.call(_this.$el, data, contentType);
                        },
                        error: function () {
                            _this.opts.error.call(_this.$el);
                        }
                    });
                }
                return false;
            });
        }
    }

    $.fn.ajaxForm = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new AjaxForm(this, options);
                rev.init();
                $(this).data('ajaxForm', rev);
            });
        }
        return this;
    };

})(jQuery);