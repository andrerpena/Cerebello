(function ($) {

    //permite adicionar uma marca d'água nos input[type='text']
    function Watermark(el, options) {

        //Defaults:
        this.defaults = {
    };

    //Extending options:
    this.opts = $.extend({}, this.defaults, options);

    //Privates:
    this.$el = $(el);
}

// Separate functionality from object creation
Watermark.prototype = {

    init: function () {
        var _this = this;

        _this.$el.bind("focus", function () {
            if (_this.$el.val() == _this.opts.text) {
                _this.applyWaterMarkFocused();
                _this.$el.setCaretPosition(0);
            }
        });

        _this.$el.bind("mouseup", function () {
            if (_this.$el.val() == _this.opts.text)
                _this.$el.setCaretPosition(0);
        });

        _this.$el.bind("blur", function () {
            if ((_this.$el.val() == '' || _this.$el.val() == _this.opts.text && _this.isWaterMarked()) || (_this.isWaterMarked() && !_this.isWaterMarkedBlured()))
                _this.applyWaterMarkBlured();
        });

        _this.$el.parents('form:first').bind("submit", function () {
            if (_this.$el.val() == _this.opts.text && _this.isWaterMarked())
                _this.$el.val('');
        });

        if ((!_this.$el.val() || _this.$el.val() == _this.opts.text) && !_this.$el.attr("disabled"))
            _this.applyWaterMarkBlured();
    },

    isWaterMarkedBlured: function () {
        var _this = this;
        return this.$el.hasClass("input-water-mark-blured") || _this.$el.hasClass("input-water-mark-blured");
    },

    isWaterMarked: function () {
        var _this = this;
        var attr = _this.$el.attr('data-val-watermarked');
        return typeof attr !== 'undefined' && attr !== false;
    },

    //aplica o water-mark no input. Substituindo o conteúdo do elemento
    applyWaterMarkBlured: function () {
        var _this = this;
        _this.$el.addClass("input-water-mark-blured").removeClass("input-water-mark-focused");
        _this.$el.val(_this.opts.text);
        _this.$el.attr("data-val-watermarked", "true");
    },

    //aplica o water-mark no input. Substituindo o conteúdo do elemento
    applyWaterMarkFocused: function () {
        var _this = this;
        _this.$el.removeClass("input-water-mark-blured").addClass("input-water-mark-focused");
        _this.$el.val(_this.opts.text);
        _this.$el.bind("keydown", function () {
            _this.removeWaterMark();
        });
        _this.$el.attr("data-val-watermarked", "true");
    },

    //remove o water-mark do input. Limpando o conteúdo do elemento
    removeWaterMark: function () {
        var _this = this;

        if (_this.$el.hasClass("input-water-mark-focused")) {
            _this.$el.removeClass("input-water-mark-focused").removeClass("input-water-mark-blured");
            _this.$el.val('');
            _this.$el.removeAttr("data-val-watermarked");
        }
    }
};

// The actual plugin
$.fn.watermark = function (options) {
    if (this.length) {
        this.each(function () {
            var rev = new Watermark(this, options);
            rev.init();
            $(this).data('watermark', rev);
        });
    }
    return this;
};
})(jQuery);