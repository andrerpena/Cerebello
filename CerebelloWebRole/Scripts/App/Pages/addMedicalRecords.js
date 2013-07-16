function AddMedicalRecords(a) {
    $(function (e) {

        AddMedicalRecords.datesWithMedicalRecords = new Array();
        AddMedicalRecords.fetchDatePickerEvents = function (year, month) {
            if (!year || !month) {
                var currentTime = $(".datepicker").datepicker("getDate");
                year = currentTime.getFullYear();
                month = currentTime.getMonth() + 1;
            }
            $.ajax({
                url: a.getDatesWithMedicalRecordsUrl,
                data: { year: year, month: month },
                success: function (result) {
                    AddMedicalRecords.datesWithMedicalRecords = result;
                    $(".datepicker").datepicker("refresh");
                }
            });
        };

        $(".datepicker").datepicker({
            showOtherMonths: false,
            selectOtherMonths: false,
            onSelect: function (dateText, inst) {
                var match = dateText.match(/(\d{2})\/(\d{2})\/(\d{4})/);
                var y = parseInt(match[3], 10),
                    m = parseInt(match[2], 10),
                    d = parseInt(match[1], 10);
                window.location = a.url + (a.url.indexOf("?") >= 0 ? "&" : "?") + "y=" + y + "&m=" + m + "&d=" + d;
            },
            beforeShowDay: function (date) {
                return [
                    // indicates that the date is selectable
                    true,
                    // sets a css class to this date item
                    jQuery.inArray(
                        $.datepicker.formatDate("'d'dd_mm_yy", date),
                        AddMedicalRecords.datesWithMedicalRecords
                    ) != -1 ? "dateWithAppointments" : ""
                ];
            },
            onChangeMonthYear: function (year, month, inst) {
                AddMedicalRecords.fetchDatePickerEvents(year, month);
            }
        });

        AddMedicalRecords.fetchDatePickerEvents();
    });
}
