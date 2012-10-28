(function ($) {

    $.addLongPollingListener = function (providerName, success, error) {
        /// <summary>Adds a listener to the long polling</summary>
        /// <param name="providerName" type="String">Provider name</param>
        /// <param name="success" type="Function">Handler that will be called when there are new events</param>
        /// <param name="error" type="Function"> Handler that will be called when an error ocurred processing the request.</param>
        if (!$._longPollingData)
            $._longPollingData = {
                listeners: new Object(),
                started: false,
                lastFetchTimeStamp: 0
            };
        if ($._longPollingData.listeners[providerName])
            throw "Cannot add long polling listener. There's already a listener for the provider name. Provider name: " + providerName;
        $._longPollingData.listeners[providerName] = new Object();
        $._longPollingData.listeners[providerName].success = success;
        $._longPollingData.listeners[providerName].error = error;
    };

    $.startLongPolling = function (longPollingUrl) {
        /// <summary>Starts the long polling</summary>
        /// <param name="longPollingUrl" type="String">The long polling Url</param>
        if (!$._longPollingData)
            throw "Cannot start long polling. There's no registered listeners";
        if ($._longPollingData.started)
            throw "Cannot start long polling. It has already started";

        $._longPollingData.started = true;

        function doLongPollingRequest() {
            $.ajax({
                url: longPollingUrl,
                cache: false,
                data: {
                    timestamp: $._longPollingData.lastFetchTimeStamp
                },
                success: function(data, s) {
                    $._longPollingData.lastFetchTimeStamp = data.Timestamp;
                    for (var i = 0; i < data.Events.length; i++) {
                        var event = data.Events[i];
                        var listener = $._longPollingData.listeners[event.ProviderName].success;
                        try {
                            listener(event);
                        } catch(ex) {
                            throw "Long polling listener triggered an Exception: " + ex;
                        }
                    };
                    doLongPollingRequest();
                },
                error: function() {
                    for (var providerName in $._longPollingData.listeners) {
                        if ($._longPollingData.listeners[providerName].error)
                            $._longPollingData.listeners[providerName].error.apply(this, arguments);
                    };
                    // an error ocurred but life must go on
                    setTimeout(doLongPollingRequest, 10000);
                }
            });
        }

        doLongPollingRequest();
    };

})(jQuery);
