function notifyMedicalAppointment(id, patientId, patientName, doctorId, doctorName, appointmentId, time, practiceIdentifier, doctorIdentifier) {
    /// <summary>notify the SECRETARY about a patient that should have come</summary>
    /// <param name="id" type="Number">notification id</param>
    /// <param name="patientId" type="patient id">patient id</param>
    /// <param name="patientName" type="String">patient name</param>
    /// <param name="doctorId" type="id">doctor id</param>
    /// <param name="doctorName" type="string">doctor name</param>
    /// <param name="appointmentId" type="string">appointmentId</param>
    /// <param name="time" type="string">appointment time spelled</param>

    var patientUrl = "/p/" + practiceIdentifier + "/d/" + doctorIdentifier + "/Patients/Details/" + patientId;

    var $newAppointmentPanel = $("<div/>").addClass("appointment-notification-panel");

    var $patientLink = $("<a/>").addClass("patient").attr("href", patientUrl).text(patientName);
    var $timeSpan = $("<span/>").addClass("time").text(time);

    var $text = $("<div/>").addClass("text").appendTo($newAppointmentPanel);
    $text.append($patientLink);
    $text.append(" possui uma consulta marcada com " + doctorName + " às ");
    $text.append($timeSpan);

    $("<a/>").attr("href", "#").text("O paciente chegou").appendTo($newAppointmentPanel).click(function (e) {
        e.preventDefault();
        $(this).trigger("notification-close");
        $.connection.notificationsHub.server.patientArrived(appointmentId, time);
        $.connection.notificationsHub.server.closeNotification(id);
    });

    $("<a/>").attr("href", "#").text("O paciente não irá comparecer").appendTo($newAppointmentPanel).click(function () {
        e.preventDefault();
        if (confirm("A consulta será considerada não realizada. Deseja continuar?")) {
            $(this).trigger("notification-close");
            $.connection.notificationsHub.server.patientWillNotArrive(appointmentId);
            $.connection.notificationsHub.server.closeNotification(id);
        }
    });

    $.notify($newAppointmentPanel, undefined, undefined, true, null, function () {
        $.connection.notificationsHub.server.closeNotification(id);
    });
}

function notifyGenericAppointment(id, text, time) {
    /// <summary>notify the DOCTOR about an upcoming generic appointment</summary>
    /// <param name="id" type="id">appointment id</param>
    /// <param name="text" type="String">appointment text/description</param>
    /// <param name="time" type="String">appointment time spelled</param>

    var $newAppointmentPanel = $("<div/>").addClass("appointment-notification-panel");
    var $timeSpan = $("<span/>").addClass("time").text(time);
    var $descriptionSpan = $("<span/>").addClass("description").text(text);

    var $text = $("<div/>").addClass("text").appendTo($newAppointmentPanel);
    $text.append("Você possui um compromisso às ");
    $text.append($timeSpan);
    $text.append(": ");
    $text.append($descriptionSpan);

    $("<a/>").attr("href", "#").text("Descartar").appendTo($newAppointmentPanel).click(function (e) {
        e.preventDefault();
        $(this).trigger("notification-close");
        $.connection.notificationsHub.server.closeNotification(id);
    });

    $.notify($newAppointmentPanel, undefined, undefined, false, null, function () {
        $.connection.notificationsHub.server.closeNotification(id);
    });
}

function notifyPatientArrived(id, patientId, patientName, time, practiceIdentifier, doctorIdentifier) {
    /// <summary>notify the DOCTOR the patient has arrived</summary>
    /// <param name="id" type="Number">notification id</param>
    /// <param name="patientId" type="Number">patient id</param>
    /// <param name="patientName" type="String">patient name</param>
    /// <param name="time" type="String">appointment time spelled</param>

    var patientUrl = "/p/" + practiceIdentifier + "/d/" + doctorIdentifier + "/Patients/Details/" + patientId;


    var $newAppointmentPanel = $("<div/>").addClass("appointment-notification-panel");

    var $patientLink = $("<a/>").addClass("patient").attr("href", patientUrl).text(patientName);
    var $timeSpan = $("<span/>").addClass("time").text(time);

    var $text = $("<div/>").addClass("text").appendTo($newAppointmentPanel);
    $text.append($patientLink);
    $text.append(" chegou para uma consulta às ");
    $text.append($timeSpan);

    $.notify($newAppointmentPanel, undefined, undefined, true, null, function () {
        $.connection.notificationsHub.server.closeNotification(id);
    });
}

function notifyNewUserCreated(id, newUserName) {
    /// <summary>notify the user a new user has been created</summary>
    $.notify("O usuário " + newUserName + " foi criado com successo. A senha padrão é: 123abc. O usuário terá a oportunidade de modificar esta senha no primeiro acesso.",
        undefined, undefined, true, null, function () {
            $.connection.notificationsHub.server.closeNotification(id);
        });
}

function notifyCompleteInfo(id) {
    /// <summary>notify the user about completing information after creating account</summary>

    var $text = $("<div/>");

    $text.append($("<p>").html("Bem vindo ao Cerebello!"));
    $text.append($("<p>").html("Antes de começar, é importante que você complete as informações sobre seu usuário e seu consultório."));
    $text.append($("<p>").html("Para agendar e realizar consultas, assim como visualizar os perfis dos pacientes, é necessário selecionar um médico primeiro. Selecione um na seção 'Médicos do consultório'."));
    $text.append($("<p>").html("O backup automático está desabilitado até que você configure sua conta do Dropbox. Para isto, dentro do perfil do médico, acesse o menu de configurações."));
    
    $.notify($text,
        undefined, undefined, true, null, function () {
            $.connection.notificationsHub.server.closeNotification(id);
        });
}

function notifyDropboxAssociated(id) {
    /// <summary>notify the user that the account now is associated</summary>

    var $text = $("<div/>");

    $text.append($("<p>").html("Sua conta agora está associada ao Dropbox e fazendo backup automaticamente de todas as informações clínicas dos pacientes."));
    $text.append($("<p>").html("As informações dos seus pacientes agora estão disponíveis em quaisquer dispositivos vinculados à sua conta do Dropbox: Seu computador pessoal, tablet, smartphone e etc."));
    $text.append($("<p>").html("Você é responsável pela segurança destas informações. Não vincule o Cerebello a uma conta do Dropbox à qual outras pessoas possuem acesso."));
    
    $.notify($text,
        undefined, undefined, true, null, function () {
            $.connection.notificationsHub.server.closeNotification(id);
        });
}

function notifyDropboxDesassociated(id) {
    /// <summary>notify the user that the account now is associated</summary>

    var $text = $("<div/>");

    $text.append($("<p>").html("A associação da sua conta ao Dropbox foi removida."));
    $text.append($("<p>").html("As informações dos seus pacientes não estão mais sendo enviadas para o Dropbox automaticamente. Todas as informações previamente enviadas serão mantidas no Dropbox e devem ser removidas manualmente."));

    $.notify($text,
        undefined, undefined, true, null, function () {
            $.connection.notificationsHub.server.closeNotification(id);
        });
}


(function ($) {

    $.connection.notificationsHub.client.notify = function (id, type, data) {
        switch (type) {
            case "GENERIC_APPOINTMENT":
                notifyGenericAppointment(id, data.Text, data.Time);
                break;
            case "MEDICAL_APPOINTMENT":
                notifyMedicalAppointment(id, data.PatientId, data.PatientName, data.DoctorId, data.DoctorName, data.AppointmentId, data.Time, data.PracticeIdentifier, data.DoctorIdentifier);
                break;
            case "PATIENT_ARRIVED":
                notifyPatientArrived(id, data.PatientId, data.PatientName, data.Time, data.PracticeIdentifier, data.DoctorIdentifier);
                break;
            case "NEW_USER":
                notifyNewUserCreated(id, data.UserName);
                break;
            case "COMPLETE_INFO":
                notifyCompleteInfo(id);
                break;
            case "DROPBOX_ASSOCIATED":
                notifyDropboxAssociated(id);
                break;
            case "DROPBOX_DESASSOCIATED":
                notifyDropboxDesassociated(id);
                break;
            default:
                throw "Invalid notification type";
        }
    };

    if (!window.hubReady)
        window.hubReady = $.connection.hub.start();

    window.hubReady.done(function () {

    });

})(jQuery);
