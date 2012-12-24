(function ($) {

    $.addLongPollingListener("new-medical-appointment",
        function (event) {
            $.notifyNewMedicalAppointment(event.EventKey, event.Data);
            // we need to tell the server this appointment has been polled successfully
            $.ajax({
                url: event.Data.AppointmentIsPolledUrl,
                error: function () {
                    throw "There was an error marking the appointment as polled";
                }
            });
        });

    // The actual plugin
    $.notifyNewMedicalAppointment = function (notificationKey, notificationData) {
        /// <summary>Shows a notification warning about an appointment about to happen</summary>
        /// <param name="notificationKey" type="String">the notification key</param>
        /// <param name="notificationData" type="Object">notification data</param>

        if (!notificationKey)
            throw "New appointment notifications must have a key";
        
        if (!notificationData)
            throw "New appointment notification data is missing";

        var $newAppointmentPanel = $("<div/>").addClass("appointment-notification-panel");

        var $patientLink = $("<a/>").addClass("patient").attr("href", notificationData.PatientUrl).text(notificationData.PatientName);
        var $doctorLink = $("<a/>").addClass("doctor").attr("href", notificationData.DoctorUrl).text(notificationData.DoctorName);
        var $timeSpan = $("<span/>").addClass("time").text(notificationData.AppointmentTime);

        var $text = $("<div/>").addClass("text").appendTo($newAppointmentPanel);
        $text.append($patientLink);
        $text.append(" possui uma consulta marcada com ");
        $text.append($doctorLink);
        $text.append(" às ");
        $text.append($timeSpan);

        $("<a/>").attr("href", "#").text("O paciente chegou").appendTo($newAppointmentPanel).click(function (e) {
            var _this = $(this);
            e.preventDefault();
            $.ajax({
                url: notificationData.AppointmentAccomplishedUrl,
                success: function () {
                    _this.trigger("notification-close");
                },
                error: function () {
                    throw "There was an error marking the appointment as accomplished";
                }
            });
        });

        $("<a/>").attr("href", "#").text("O paciente não irá comparecer").appendTo($newAppointmentPanel).click(function () {
            if (confirm("A consulta será considerada não realizada. Deseja continuar?")) {
                var _this = $(this);
                $.ajax({
                    url: notificationData.AppointmentCanceledUrl,
                    success: function () {
                        _this.trigger("notification-close");
                    },
                    error: function () {
                        throw "There was an error marking the appointment as NotAccomplished";
                    }
                });
            }
        });

        $.notify($newAppointmentPanel, undefined, undefined, false, notificationKey);
    };

})(jQuery);
