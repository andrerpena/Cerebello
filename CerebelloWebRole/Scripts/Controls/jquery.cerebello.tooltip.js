(function ($) {

    // provides client-side paging
    function Tooltip(el, options) {
        var _this = this;

        // Defaults:
        _this.defaults = {
            text: ''
        };

        if (options.currentPageIndex == null)
            options.currentPageIndex = undefined;

        //Extending options:
        _this.opts = $.extend({}, this.defaults, options);

        //Privates:
        _this.$el = $(el);
        _this.$dropdown = null;
        _this.dropdownId = "tooltip_" + generateGuid("_") + "_dropdown";

        _this.createDropdown = function () {

            _this.$dropdown = $("<div/>").attr("id", _this.dropdownId).addClass("tooltip-dropdown").appendTo($("body"));
            _this.$dropdown.text(_this.opts.text);
            $('html').bind("click", function (e) {
                if (_this.$el[0] != e.target && _this.$dropdown[0] != e.target)
                    _this.hideDropdown();
            });
        };

        _this.isDropdownCreated = function () {
            return this.$dropdown != null;
        };

        _this.isDropdownVisible = function () {
            return _this.$dropdown && _this.$dropdown.is(":visible");
        };

        // shows the dropdown and fixes it's position
        _this.showAndFixDropdownPosition = function () {
            var _this = this;
            if (!_this.isDropdownVisible())
                _this.$dropdown.show();

            _this.$el.addClass("selected");
            _this.$dropdown.css("left", _this.$el.offset().left);
            _this.$dropdown.css("top", _this.$el.offset().top + _this.$el.outerHeight());
            _this.$dropdown.css("min-width", 120);
        };

        _this.hideDropdown = function () {
            _this.$el.removeClass("selected");
            _this.$dropdown.hide();
        };


    }

    // Separate functionality from object creation
    Tooltip.prototype = {
        init: function () {
            var _this = this;
            _this.$el.html("?");
            _this.$el.click(function () {
                if (!_this.isDropdownCreated())
                    _this.createDropdown();
                if (!_this.isDropdownVisible())
                    _this.showAndFixDropdownPosition();
                else
                    _this.hideDropdown();
            });
        }
    };

    // The actual plugin
    $.fn.tooltip = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new Tooltip(this, options);
                rev.init();
                $(this).data('tooltip', rev);
            });
        }
        return this;
    };
})(jQuery);
