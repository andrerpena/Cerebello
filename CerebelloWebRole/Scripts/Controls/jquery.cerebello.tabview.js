(function ($) {

    // provides client-side paging
    function TabView(el, options) {

        // Defaults:
        this.defaults = {
            name: "tab",
            items: [],
            selectedIndex: 0
        };

        //Extending options:
        this.opts = $.extend({}, this.defaults, options);

        //Privates:
        this.$el = $(el);
        this.$ul = null;
    }

    // Separate functionality from object creation
    TabView.prototype = {

        init: function () {
            var _this = this;

            var $tabView = $("<div>").addClass("tab-view").appendTo(_this.$el);
            _this.$ul = $("<ul>").appendTo($tabView);
            _this.$tabContainter = $("<div>").addClass("tab-container").appendTo($tabView);

            // Bind to StateChange Event
            History.Adapter.bind(window, 'statechange', function () { // Note: We are using statechange instead of popstate
                var state = History.getState();
                if (!window.localPushState) {
                    if (state.data.ownerControl == "tabView") {
                        var $li = $("li[data-tab-key=" + state.data.tabKey + "]", _this.$ul);
                        if ($li.length)
                            _this.selectMenu($li);
                    }
                }
                window.localPushState = false;
            });

            // add items
            for (var i = 0; i < _this.opts.items.length; i++) {
                $("<li>")
                    .text(_this.opts.items[i].title)
                    .attr("data-ajax-url", _this.opts.items[i].url)
                    .attr("data-tab-key", _this.opts.items[i].key)
                    .attr("data-tab-index", i)
                    .appendTo(_this.$ul);
            }

            _this.selectMenu = function ($li) {

                // load content
                window.ajaxLoad($li.attr("data-ajax-url"), _this.$tabContainter, "fill");

                // selects the li
                $li.siblings().removeClass("selected");
                $li.addClass("selected");

                // push state
                var urlStateParameter = _this.opts.name;
                var urlStateValue = $li.attr("data-tab-key");
                window.localPushState = true;
                History.pushState(
                    {
                        ownerControl: "tabView",
                        tabKey: $li.attr("data-tab-key")
                    },
                    document.title, "?" + urlStateParameter + "=" + urlStateValue
                );

            };

            // determine the selected item
            var regex = new RegExp(_this.opts.name + "=(\\w+)");
            var selectedItemMatch = document.URL.match(regex);
            var $selectedLi = null;
            if (selectedItemMatch) {
                $selectedLi = $("li[data-tab-key=" + selectedItemMatch[1] + "]", _this.$ul);
                if ($selectedLi.length)
                    _this.selectMenu($selectedLi);
            };

            if (!$selectedLi || !$selectedLi.length)
                _this.selectMenu($("li:first", _this.$ul));

            $("li", _this.$ul).click(function (e) {
                _this.selectMenu($(this));
            });
        }
    };

    // The actual plugin
    $.fn.tabView = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new TabView(this, options);
                rev.init();
                $(this).data('tabview', rev);
            });
        }
        return this;
    };
})(jQuery);
