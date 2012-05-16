(function ($) {

    // allows the user to navigate through large data-sets
    function Lookup(el, options) {
        var _this = this;

        //Defaults:
        this.defaults = {
            contentUrl: '',
            searchParamName: 'term',
            // the pageIndex is 1-based
            pageIndexParamName: 'pageIndex',
            pageSizeParamName: 'pageSize',
            pageSize: 5,
            ajaxParams: {},
            formatItem: function () { },
            change: function (data) { },
            inputHiddenName: null,
            columnId: "Id",
            columnText: "Value",
            columns: ["Value"],
            columnHeaders: ["Valor"],
            // time until it will filter automatically
            autoFilterDelay: 1000
        };

        //Extending options:
        _this.opts = $.extend({}, this.defaults, options);

        //Privates:
        _this.$el = $(el);

        _this.$lookupInputHidden = null;
        _this.$lookupPagerWrapper = null;
        _this.$lookupDropdown = null;
        _this.$lookupWrapper = null;

        _this.lookupPaginator = null;
        _this.lookupDropdownId = null;

        // this is the "setInterval" handler, used for controlling auto-filter
        _this.intervalHandler = null;

        _this.isDropdownCreated = function () {
            return _this.$lookupDropdown && _this.$lookupDropdown.length > 0;
        };

        _this.isDropdownVisible = function () {
            return _this.isDropdownCreated() && _this.$lookupDropdown.is(":visible");
        };

        _this.showAndFixDropdownPosition = function () {
            if (_this.$lookupDropdown && !_this.$lookupDropdown.is(":visible"))
                _this.$lookupDropdown.show();

            _this.$lookupDropdown.css("left", _this.$el.offset().left);
            _this.$lookupDropdown.css("top", _this.$el.offset().top + _this.$el.outerHeight() + 3);
            _this.$lookupDropdown.css("min-width", _this.$el.outerWidth() + 100);
        };

        _this.moveFocusForward = function () {
            var selected = $("tbody > tr.selected", _this.$lookupWrapper);
            // se já existe uma linha selecionada
            if (selected.length) {
                var next = selected.next("tbody > tr");
                if (next.length)
                    next.addClass("selected").siblings().removeClass("selected");
                else
                    _this.lookupPaginator.nextPage(function () {
                        var firstTr = $("tbody > tr", _this.$lookupWrapper).filter(":first");
                        if (firstTr.length)
                            firstTr.addClass("selected").siblings().removeClass("selected");
                    });
            }
            else {
                var firstTr = $("tbody > tr", _this.$lookupWrapper).eq(0);
                if (firstTr.length)
                    firstTr.addClass("selected").siblings().removeClass("selected");
            }
        };

        _this.moveFocusBackward = function () {
            var selected = $("tbody > tr.selected", _this.$lookupWrapper);
            if (selected.length) {
                var prev = selected.prev("tbody > tr");
                if (prev.length)
                    prev.addClass("selected").siblings().removeClass("selected");
                else if (_this.lookupPaginator.prevPage(function () {
                    var lastTr = $("tbody > tr", _this.$lookupWrapper).filter(":last");
                    if (lastTr.length)
                        lastTr.addClass("selected").siblings().removeClass("selected");
                })) {

                }
                else
                    $("tbody > tr", _this.$lookupWrapper).removeClass("selected");
            }
        };

        // creates a new Id for dropdown
        _this.createDropdownId = function () {
            var s4 = function () {
                return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
            };
            return "lookup_" + (s4() + s4() + "_" + s4() + "_" + s4() + "_" + s4() + "_" + s4() + s4() + s4()) + "_dropdown";
        };

        _this.selectCurrentlyFocusedRow = function () {
            var selectedRow = $("tr.selected", _this.$lookupWrapper);
            var selectedText = $("td", selectedRow).html();
            var selectedId = selectedRow.attr("data-val-id");
            _this.textCache = selectedText;
            _this.$el.val(selectedText);
            _this.$lookupInputHidden.val(selectedId);
            _this.$lookupDropdown.hide();
            _this.opts.change(selectedRow.data("lookup-row"));
        };

        // gets data from the server and shows the dropdown 
        _this.getData = function (pageIndex, onDataReceived, searchTerm) {

            if (!_this.isDropdownCreated()) {

                $("body").append($("<div id='" + _this.lookupDropdownId +
                "' class='lookup-dropdown'><div class='lookup-wrapper'></div><div class='lookup-pager-wrapper'></div></div>"));

                _this.$lookupDropdown = $("#" + _this.lookupDropdownId);

                _this.$lookupWrapper = $(".lookup-wrapper", _this.$lookupDropdown);
                _this.$lookupPagerWrapper = $(".lookup-pager-wrapper", _this.$lookupDropdown);

                $('html').bind("click", function (e) {
                    if (!_this.$el.has(e.target).length && !_this.$lookupWrapper.parent().has(e.target).length)
                        _this.$lookupDropdown.hide();
                });
            }

            _this.$lookupWrapper.html("<div class='lookup-loading'></div>");

            if (_this.lookupPaginator != null)
                _this.lookupPaginator.opts.enabled = false;

            _this.showAndFixDropdownPosition();

            var params = new Object();
            $.extend(params, _this.opts.ajaxParams);

            if (searchTerm)
                params[_this.opts.searchParamName] = searchTerm;

            params[_this.opts.pageIndexParamName] = pageIndex ? pageIndex : 1;
            params[_this.opts.pageSizeParamName] = _this.opts.pageSize;

            $.ajax({
                url: _this.opts.contentUrl,
                data: params,
                dataType: 'json',
                success: function (data) {
                    // criar a tabela
                    var rows = [];

                    if (data.Rows && data.Rows.length > 0) {
                        var $table = $("<table />");

                        // header
                        if (_this.opts.columnHeaders.length > 1) {
                            var $tableHead = $("<thead />").appendTo($table);
                            $tableHead.append("<tr />");
                            for (var i = 0; i < _this.opts.columnHeaders.length; i++)
                                $("tr", $tableHead).append("<td>" + _this.opts.columnHeaders[i] + "</td>");
                        }

                        // body
                        var $tableBody = $("<tbody>").appendTo($table);
                        $.each(data.Rows, function (key, val) {
                            var $row = $("<tr data-val-id='" + val[_this.opts.columnId] + "'></tr>");

                            for (var i = 0; i < _this.opts.columns.length; i++) {
                                var column = _this.opts.columns[i];
                                if (val[column] != null && val[column] != undefined)
                                    $row.append("<td>" + val[column] + "</td>");
                                else
                                    $row.append("<td></td>");
                            }

                            $row.data("lookup-row", val);
                            $tableBody.append($row);
                        });

                        _this.$lookupWrapper.html($table);
                    }
                    else
                        _this.$lookupWrapper.html("<div class='empty-box'>A pesquisa não retornou registros</div>");

                    $("tbody > tr", _this.$lookupWrapper).bind("click", function () {
                        $(this).addClass("selected").siblings().removeClass("selected");
                        _this.selectCurrentlyFocusedRow();
                    });

                    _this.lookupPaginator = _this.$lookupPagerWrapper.pager({
                        count: data.Count,
                        rowsPerPage: _this.opts.pageSize,
                        currentPageIndex: pageIndex,
                        onPageChanged: function (i, onDataReceived) {
                            // ativar o activity indicator
                            _this.getData(i, onDataReceived, searchTerm);
                        }
                    }).data('pager');

                    _this.lookupPaginator.opts.enabled = true;

                    if (onDataReceived)
                        onDataReceived();
                },
                error: function () {
                    _this.$lookupWrapper.html("<div class='empty-box'>Não é possível exibir o resultado. Erro no servidor</div>");
                }
            });
        };

        _this.clear = function () {
            _this.$el.val('');
            _this.$lookupInputHidden.val('');
            $("tr", _this.$lookupWrapper).removeClass("selected");
            _this.opts.change(undefined);
        };
    }

    // Separate functionality from object creation
    Lookup.prototype = {

        init: function () {

            var _this = this;

            _this.$lookupInputHidden = $("input[name='" + _this.opts.inputHiddenName + "']");
            if (!_this.$lookupInputHidden.length)
                throw "Couldn't find the inputHiddenName";

            _this.lookupDropdownId = _this.createDropdownId();

            _this.$el.bind("blur", function (e) {
                $("tr", _this.$lookupWrapper).removeClass("selected");
            });

            _this.$el.bind("keydown", function (e) {
                _this.textCache = _this.$el.val();
                switch (e.keyCode) {

                    // enter                                                                                                                                                                                                                               
                    case 13:
                        e.preventDefault();

                        if (_this.intervalHandler)
                            clearTimeout(_this.intervalHandler);

                        if (_this.isDropdownVisible() && $("tr.selected", _this.$lookupWrapper).length)
                            _this.selectCurrentlyFocusedRow();
                        // filter
                        else
                            _this.getData(null, null, _this.$el.val());
                        break;
                    // seta pra baixo                                                                                                                                                                           
                    case 40:
                        if (!_this.isDropdownVisible())
                            _this.getData(null, function () { _this.moveFocusForward(); }, _this.$el.val());
                        else {
                            _this.showAndFixDropdownPosition();
                            _this.moveFocusForward();
                        }
                        break;
                    // seta para cima                                                                                                                                                                                                                        
                    case 38:
                        if (_this.isDropdownCreated()) {
                            _this.showAndFixDropdownPosition();
                            _this.moveFocusBackward();
                        }
                        break;
                    // esc e tab             
                    case 27:
                    case 9:
                        if (_this.isDropdownCreated())
                            _this.$lookupDropdown.hide();
                        break;
                    default:
                        $("tr", _this.$lookupWrapper).removeClass("selected");
                        break;
                }
            });

            _this.$el.bind("keyup", function (e) {
                if (_this.$el.val() != _this.textCache) {
                    _this.$lookupInputHidden.val('');
                    _this.opts.change(undefined);
                    if (_this.intervalHandler)
                        clearTimeout(_this.intervalHandler);

                    _this.intervalHandler = setTimeout(function () {
                        _this.getData(null, null, _this.$el.val());
                    }, _this.opts.autoFilterDelay);
                }
            });
        }
    };

    // The actual plugin
    $.fn.lookup = function (options) {
        if (this.length) {
            this.each(function () {
                var rev = new Lookup(this, options);
                rev.init();
                $(this).data('lookup', rev);
            });
        }
        return this;
    };

})(jQuery);
