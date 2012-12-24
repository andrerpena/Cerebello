(function ($) {

    $.addLongPollingListener("new-generic-appointment",
        function (event) {
            $.notifyNewGenericAppointment(event.EventKey, event.Data);
            // we need to tell the server this appointment has been polled successfully
            $.ajax({
                url: event.Data.AppointmentIsPolledUrl,
                error: function () {
                    throw "There was an error marking the appointment as polled";
                }
            });
        });

    // The actual plugin
    $.notifyNewGenericAppointment = function (notificationKey, notificationData) {
        /// <summary>Shows a notification warning about an appointment about to happen</summary>
        /// <param name="notificationKey" type="String">the notification key</param>
        /// <param name="notificationData" type="Object">notification data</param>

        if (!notificationKey)
            throw "New appointment notifications must have a key";
        
        if (!notificationData)
            throw "New appointment notification data is missing";

        var $newAppointmentPanel = $("<div/>").addClass("appointment-notification-panel");

        var $timeSpan = $("<span/>").addClass("time").text(notificationData.AppointmentTime);
        var $descriptionSpan = $("<span/>").addClass("description").text(notificationData.Description);

        var $text = $("<div/>").addClass("text").appendTo($newAppointmentPanel);
        $text.append("Você possui um compromisso às ");
        $text.append($timeSpan);
        $text.append(": ");
        $text.append($descriptionSpan);

        $("<a/>").attr("href", "#").text("Descartar").appendTo($newAppointmentPanel).click(function (e) {
            var _this = $(this);
            e.preventDefault();
            $.ajax({
                url: notificationData.AppointmentDiscardedUrl,
                success: function () {
                    _this.trigger("notification-close");
                },
                error: function () {
                    throw "There was an error marking the appointment as discarded";
                }
            });
        });

        $.notify($newAppointmentPanel, undefined, undefined, false, notificationKey);
    };

})(jQuery);
