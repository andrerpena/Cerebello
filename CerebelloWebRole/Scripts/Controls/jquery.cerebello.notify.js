(function ($) {

    // The actual plugin
    $.notify = function (content, cssClass, timeout, canClose, notificationKey, onClose) {
        /// <summary>Shows a client-side content</summary>
        /// <param name="content" type="Object">May be plain text or a jQuery object</param>
        /// <param name="cssClass" type="String">Custom CSS class</param>
        /// <param name="timeout" type="Number">Time until it will close automatically</param>
        /// <param name="canClose" type="Boolean">Whether or not to show the X button. Defaults to true</param>
        /// <param name="onClose" type="Function">Function that is called after the notiication is closed</param>

        // if there's no notifications-wrapper, create it
        var $notificationsWrapper = $(".notifications-wrapper");
        if (!$notificationsWrapper.length)
            $notificationsWrapper = $("<div/>").addClass("notifications-wrapper").appendTo($("body"));

        // if there's a notification key specified, verifies whether or not 
        // it already exists
        if (notificationKey) {
            var existingNotification = $("div[data-val-key='" + notificationKey + "']");
            if (existingNotification.length)
                return;
        }

        // creates the DOM elements for the new notification
        var $notification = $("<div/>").addClass("notification").css("display", "none").attr("data-val-key", notificationKey).appendTo($notificationsWrapper).bind("notification-close", function () {
            $notification.fadeOut("fast", function () {
                $notification.remove();
                if (onClose)
                    onClose();
            });
        });
        
        if (cssClass)
            $notification.addClass(cssClass);

        if (canClose != false)
            $("<div/>").addClass("close").appendTo($notification).click(function () {
                $(this).trigger("notification-close");
            });

        $("<div/>").addClass("notification-content").html(content).appendTo($notification);

        if (timeout)
            setTimeout(function () {
                $notification.trigger("notification-content");
            }, timeout);

        $notification.fadeIn("fast");
    };

})(jQuery);
