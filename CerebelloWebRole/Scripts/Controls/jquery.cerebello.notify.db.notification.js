(function ($) {

    $.addLongPollingListener("notifications",
        function (event) {
            $.notifyDb(event.EventKey, event.Data);
            // we need to tell the server this notification has been polled successfully
            $.ajax({
                url: event.Data.NotificationIsPolledUrl,
                error: function () {
                    throw "There was an error marking a notification as polled";
                }
            });
        });

    // The actual plugin
    $.notifyDb = function (notificationKey, notificationData) {
        /// <summary>Shows a notification warning about an appointment about to happen</summary>
        /// <param name="notificationKey" type="String">the notification key</param>
        /// <param name="notificationData" type="Object">notification data</param>

        if (!notificationKey)
            throw "New appointment notifications must have a key";

        if (!notificationData)
            throw "New appointment notification data is missing";

        $.notify(notificationData.Text, undefined, undefined, true, notificationKey, function () {
            (function () {
                $.ajax({
                    url: notificationData.NotificationRemoveUrl,
                    error: function() {
                        throw "There was an error removing a notification";
                    }
                });
            })();
        });
    };

})(jQuery);
