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
        init: function() {
            var _this = this;
            $.validator.unobtrusive.parse(_this.$el);
            _this.$el.submit(function() {
                if ($(this).valid()) {
                    var formData = $(this).serialize();
                    $.ajax({
                        url: this.action,
                        type: this.method,
                        data: formData,
                        success: function(data, status, e) {
                            var contentTypeHeader = e.getResponseHeader("Content-Type");
                            var contentType;
                            if (contentTypeHeader.indexOf("text/html") != -1)
                                contentType = "html";
                            else if (contentTypeHeader.indexOf("application/json") != -1)
                                contentType = "json";
                            _this.opts.success.call(_this.$el, data, contentType);
                        },
                        error: function() {
                            _this.opts.error.call(_this.$el);
                        }
                    });
                }
                return false;
            });
        }
    };

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

    $(document).on("click", function (e) {
        var prUrl = $(e.target).attr("data-pr-url");
        if (!prUrl)
            return;
        var prTarget = $(e.target).attr("data-pr-target");
        if (!prTarget)
            throw "When data-pr-url is defined, data-pr-target must be defined too";
        var $target = $(prTarget);
        if (!$target.length)
            return;
        var prBehavior = $(e.target).attr("data-pr-behavior");
        if (!prBehavior)
            prBehavior = "replace";


    });

})(jQuery);