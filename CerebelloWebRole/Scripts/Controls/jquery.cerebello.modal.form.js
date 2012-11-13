(function ($) {

    function ModalForm(el, options) {

        //Defaults:
        this.defaults = {
            
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = $(el);
    }

    ModalForm.prototype = {

        init: function () {
            var _this = this;

            var $formWrapper = _this.$el.parent();

            _this.$el.ajaxForm({
                success: function (result, contentType) {
                    if (contentType == "json")
                        _this.$el.trigger("modal-ok", result);
                    else {
                        $formWrapper.replaceWith(result);
                        $formWrapper.trigger("modal-resize");
                    }
                }
            });
        }
    }

    $.fn.modalForm = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new ModalForm(this, options);
                rev.init();
                $(this).data('modalForm', rev);
            });
        }
        return this;
    };

})(jQuery);