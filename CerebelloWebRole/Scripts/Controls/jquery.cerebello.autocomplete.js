(function ($) {

    // allows the user to navigate through large data-sets
    function Autocomplete(el, options) {
        var _this = this;

        //Defaults:
        this.defaults = {
            // content
            contentUrl: '',
            // params
            searchParamName: 'term',
            pageIndexParamName: 'pageIndex',
            pageSizeParamName: 'pageSize',

            pageSize: 5,
            ajaxParams: {},

            newWindowUrl: null,
            newWindowMinHeight: null,
            newWindowWidth: null,
            newWindowTitle: null,

            inputHiddenName: null,
            columnId: "Id",
            columnText: "Value",
            columns: ["Value"],
            columnHeaders: ["Valor"],
            // time until it will filter automatically
            autoFilterDelay: 500,
            //events
            change: function () { },
            formatItem: function () { }
        };

        //Extending options:
        _this.opts = $.extend({}, this.defaults, options);

        //Privates:
        _this.$el = $(el);

        _this.pager = null;
        _this.dropdownId = null;
        // this is the "setInterval" handler, used for controlling auto-filter
        _this.intervalHandler = null;
        _this.textCache = null;

        // jQueries
        _this.$wrapper = null;
        _this.$dropdown = null;
        _this.$pagerWrapper = null;
        _this.$inputHidden = null;

    }

    // Separate functionality from object creation
    Autocomplete.prototype = {

        init: function () {

            var _this = this;

            // add the wrapper and the new button
            if (_this.opts.newWindowUrl)
                _this.$el.wrap($("<span/>").addClass("autocomplete-text-wrapper")).after($('<span/>').addClass("new-button").click(function () {

                    $.modal({
                        url: _this.opts.newWindowUrl,
                        width: _this.opts.newWindowWidth,
                        height: _this.opts.newWindowMinHeight,
                        title : _this.opts.newWindowTitle
                    });

                }));

            _this.$inputHidden = $("input[name='" + _this.opts.inputHiddenName + "']");
            if (!_this.$inputHidden.length)
                throw "Couldn't find the $inputHiddenName";

            _this.dropdownId = "lookup_" + generateGuid("_") + "_dropdown";

            _this.$el.bind("blur", function (e) {
                $("tr", _this.$wrapper).removeClass("selected");
            });

            _this.$el.bind("keydown", function (e) {
                _this.textCache = _this.$el.val();
                switch (e.keyCode) {

                    // enter                                                                                                                                                                                                                               
                    case 13:
                        e.preventDefault();

                        if (_this.intervalHandler)
                            clearTimeout(_this.intervalHandler);

                        if (_this.isDropdownVisible() && $("tr.selected", _this.$wrapper).length)
                            _this.selectCurrentlyFocusedRow();
                            // filter
                        else
                            _this.fetchData(null, null, _this.$el.val());
                        break;
                        // seta pra baixo                                                                                                                                                                           
                    case 40:
                        e.preventDefault();
                        if (!_this.isDropdownVisible())
                            _this.fetchData(null, function () { _this.focusNextRow(); }, _this.$el.val());
                        else {
                            _this.showAndFixDropdownPosition();
                            _this.focusNextRow();
                        }
                        break;
                        // seta para cima                                                                                                                                                                                                                        
                    case 38:
                        e.preventDefault();
                        if (_this.isDropdownCreated()) {
                            _this.showAndFixDropdownPosition();
                            _this.focusPreviousRow();
                        }
                        break;
                        // esc e tab             
                    case 27:
                    case 9:
                        if (_this.isDropdownCreated())
                            _this.$dropdown.hide();
                        break;
                    default:
                        $("tr", _this.$wrapper).removeClass("selected");
                        break;
                }
            });

            _this.$el.bind("keyup", function (e) {

                if (_this.$el.val() != _this.textCache) {
                    _this.opts.change(undefined);
                    if (_this.intervalHandler)
                        clearTimeout(_this.intervalHandler);
                    _this.intervalHandler = setTimeout(function () {
                        if (_this.$el.is(":focus"))
                            _this.fetchData(null, null, _this.$el.val());
                    }, _this.opts.autoFilterDelay);
                }
            });
        },

        // returns whether the dropdown is created
        isDropdownCreated: function () {
            var _this = this;
            return _this.$dropdown && _this.$dropdown.length > 0;
        },

        // returns whether the dropdown is visible
        isDropdownVisible: function () {
            var _this = this;
            return _this.isDropdownCreated() && _this.$dropdown.is(":visible");
        },

        // makes sure the dropdown is created
        ensureDropdownIsCreated: function () {
            var _this = this;
            if (!_this.isDropdownCreated()) {

                _this.$dropdown = $("<div/>").attr("id", _this.dropdownId).addClass("autocomplete-dropdown").appendTo($("body"));
                _this.$wrapper = $("<div/>").addClass("autocomplete-dropdown-wrapper").appendTo(_this.$dropdown);
                _this.$pagerWrapper = $("<div/>").addClass("lookup-pager-wrapper").appendTo(_this.$dropdown);

                _this.$dropdown.click(function () {
                    _this.$el.focus();
                });

                $('html').bind("click", function (e) {
                    if (!_this.$el.has(e.target).length && _this.$dropdown && !_this.$dropdown.has(e.target).length)
                        _this.$dropdown.hide();
                });
            }
        },

        // shows the dropdown and fixes it's position
        showAndFixDropdownPosition: function () {
            var _this = this;
            if (_this.$dropdown && !_this.$dropdown.is(":visible"))
                _this.$dropdown.show();

            _this.$dropdown.css("left", _this.$el.offset().left);
            _this.$dropdown.css("top", _this.$el.offset().top + _this.$el.outerHeight() + 3);
            _this.$dropdown.css("min-width", _this.$el.outerWidth() + 100);
        },

        clear: function () {
            var _this = this;
            _this.$inputHidden.val('');
            _this.$el.val('');
            $("tr", _this.$wrapper).removeClass("selected");
            _this.opts.change(undefined);
        },

        // gets data from the server and shows the $dropdown 
        fetchData: function (pageIndex, onDataReceived, searchTerm) {
            var _this = this;
            _this.ensureDropdownIsCreated();
            _this.$wrapper.html($("<div/>").addClass("autocomplete-loading"));

            if (_this.pager != null)
                _this.pager.opts.enabled = false;

            _this.showAndFixDropdownPosition();

            var params = new Object();
            $.extend(params, _this.opts.ajaxParams);

            params[_this.opts.searchParamName] = searchTerm;
            params[_this.opts.pageIndexParamName] = pageIndex ? pageIndex : 1;
            params[_this.opts.pageSizeParamName] = _this.opts.pageSize;

            $.ajax({
                url: _this.opts.contentUrl,
                data: params,
                dataType: 'json',
                success: function (data) {

                    // creates the table
                    if (data.Rows && data.Rows.length > 0) {
                        var $grid = $("<table />");

                        // header
                        if (_this.opts.columnHeaders.length > 1) {
                            var $gridHeader = $("<thead />").appendTo($grid);
                            $gridHeader.append("<tr />");
                            for (var i = 0; i < _this.opts.columnHeaders.length; i++)
                                $("<td/>").html(_this.opts.columnHeaders[i]).appendTo($("tr", $gridHeader));
                        }

                        // body
                        var $tableBody = $("<tbody>").appendTo($grid);
                        $.each(data.Rows, function (key, val) {
                            var $row = $("<tr/>").attr("data-val-id", val[_this.opts.columnId]);

                            for (var j = 0; j < _this.opts.columns.length; j++) {
                                var column = _this.opts.columns[j];
                                if (val[column] != null && val[column] != undefined)
                                    $("<td/>").html(val[column]).appendTo($row);
                                else
                                    $("<td/>").appendTo($row);
                            }

                            $row.data("lookup-row", val);
                            $tableBody.append($row);
                        });

                        _this.$wrapper.html($grid);
                    }
                    else
                        _this.$wrapper.html($("<div/>").addClass("no-results-box").text("A pesquisa não retornou registros"));

                    $("tbody > tr", _this.$wrapper).bind("click", function () {
                        $(this).addClass("selected").siblings().removeClass("selected");
                        _this.selectCurrentlyFocusedRow();
                    });

                    _this.pager = _this.$pagerWrapper.pager({
                        count: data.Count,
                        rowsPerPage: _this.opts.pageSize,
                        currentPageIndex: pageIndex,
                        onPageChanged: function (i, onDataReceived2) {
                            // ativar o activity indicator
                            _this.$el.focus();
                            _this.fetchData(i, onDataReceived2, searchTerm);
                        }
                    }).data('pager');

                    _this.pager.opts.enabled = true;

                    if (onDataReceived)
                        onDataReceived();
                },
                error: function () {
                    _this.$wrapper.html("<div class='no-results-box'>Não é possível exibir o resultado. Erro no servidor</div>");
                }
            });
        },

        // focuses the next row
        focusNextRow: function () {
            var _this = this;
            function selectFirstRow() {
                $("tbody > tr", _this.$wrapper).filter(":first").addClass("selected").siblings().removeClass("selected");
            }
            var selectedRow = $("tbody > tr.selected", _this.$wrapper);
            if (selectedRow.length) {
                var nextRow = selectedRow.next("tbody > tr");
                if (nextRow.length)
                    nextRow.addClass("selected").siblings().removeClass("selected");
                else
                    _this.pager.nextPage(selectFirstRow);
            }
            else
                selectFirstRow();
        },


        // focuses the previous row
        focusPreviousRow: function () {
            var _this = this;
            var selectedRow = $("tbody > tr.selected", _this.$wrapper);
            if (selectedRow.length) {
                var previousRow = selectedRow.prev("tbody > tr");
                if (previousRow.length)
                    previousRow.addClass("selected").siblings().removeClass("selected");
                else if (!_this.pager.prevPage(function () {
                    $("tbody > tr", _this.$wrapper).filter(":last").addClass("selected").siblings().removeClass("selected");
                }))
                    $("tbody > tr", _this.$wrapper).removeClass("selected");
            }
        },

        selectCurrentlyFocusedRow: function () {
            var _this = this;
            _this.$el.removeClass("changed");
            var selectedRow = $("tr.selected", _this.$wrapper);
            var selectedText = $("td", selectedRow).html();
            var selectedId = selectedRow.attr("data-val-id");
            _this.textCache = selectedText;
            _this.$el.val(selectedText);
            _this.$inputHidden.val(selectedId);
            _this.$dropdown.hide();
            _this.opts.change(selectedRow.data("lookup-row"));
            _this.$el.focus();
        }

    };

    // The actual plugin
    $.fn.autocomplete = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new Autocomplete(this, options);
                rev.init();
                $(this).data('autocomplete', rev);
            });
        }
        return this;
    };

})(jQuery);
