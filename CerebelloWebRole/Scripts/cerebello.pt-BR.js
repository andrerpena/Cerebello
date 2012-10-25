// JavaScript resources pt-BR

cerebello = typeof cerebello == "undefined" ? {} : cerebello;

cerebello.res = {
    confirmationBase: {
        // The error message displayed when the server returns an error.
        errorMessage: "Não foi possível executar a operação {operationName}.",

        // The text used between the action button and the cancel link. e.g. "ou".
        orText: "ou",

        // The cancel text used in the cancel link. e.g. "cancelar".
        cancelText: "cancelar",

        // The action name used in the button. e.g. "resetar senha".
        actionName: "ok",

        // This field is going to be processed twice.
        // The first time it is formatted with 'options' object. Fields are like this '{field}'.
        // The second time it is formatted with 'data' object from json response. Fields are like this '{{field}}'
        techInfoText: "Informações técnicas: {{text}}",
    },

    // Strings for the delete confirmation windows.
    deleteConfirmation: {
        // Title of the window... default is the same as the operationName.
        title: function () {
            return "Excluir " + (this.objectName ? this.objectName : this.objectType) + "?";
        },

        // Text with informations about the risks of the operation, and other details about the operation.
        message: function () {
            return ("Você está tentando excluir um(a) <b>"
                + this.objectType + "</b> permanentemente" +
                // optionally adds object name descriptions if it exists
                (this.objectName ? " <b>(" + this.objectName + ")</b>" : "")
                + ". Todos os registros que dependem deste podem ser excluídos também. "
                + "Esta operação não pode ser desfeita.");
        },

        // Text instructing the user on how to process with the operation.
        confirmMessage: function () {
            if (this.checkString)
                return ("Para prosseguir com a exclusão, digite <b>"
                    + this.objectType + "</b> no campo abaixo");
            return "Se tiver certeza que deseja prosseguir com a exclusão, clique no botão <b>{actionName}</b>.";
        },

        // String that the user must type, so that the operation is executed.
        checkString: "{objectType}",

        // The error message displayed when the server returns an error.
        errorMessage: "Não foi possível excluir este(a) '{objectType}'.",

        // The action name used in the button. e.g. "resetar senha".
        actionName: "excluir",

        // The operation name. e.g. "Resetar senha", "Excluir usuário".
        operationName: "Excluir usuário",

        // Optional. The type of the object affected by the operation. Like 'usuário'.
        objectType: function () {
            throw "You must specify an objectType when deleting objects.";
        },

        // Optional. The name of the specific object affected by the operation. Like 'Jonas da Silva'.
        objectName: function () {
            throw "You must specify an objectName when deleting objects.";
        },

        // This field is going to be processed twice.
        // The first time it is formatted with 'options' object. Fields are like this '{field}'.
        // The second time it is formatted with 'data' object from json response. Fields are like this '{{field}}'
        successMessage: "Este usuário foi excluído.",
    },

    // Defaults:
    resetPasswordConfirmation: {
        // Title of the window... default is the same as the operationName.
        title: function () {
            return "Resetar senha de " + (this.objectName ? this.objectName : this.objectType) + "?";
        },

        // Text with informations about the risks of the operation, and other details about the operation.
        message: function () {
            return ("<b>{operationName}</b> <br /> O seguinte objeto será afetado: <b>{objectType}</b>" +
                // optionally adds object name descriptions if it exists
                (this.objectName ? " <b>({objectName})</b>" : "")
                + ".");
        },

        // Text instructing the user on how to process with the operation.
        confirmMessage: function () {
            if (this.checkString)
                return "Para prosseguir com a operação, digite <b>{checkString}</b> no campo abaixo, e então clique no botão <b>{actionName}</b>.";
            return "Se tiver certeza que deseja prosseguir com a operação, clique no botão <b>{actionName}</b>.";
        },

        // String that the user must type, so that the operation is executed.
        checkString: null,

        // The error message displayed when the server returns an error.
        errorMessage: "Não foi possível executar a operação {operationName}.",

        // The action name used in the button. e.g. "resetar senha".
        actionName: "resetar senha",

        // The operation name. e.g. "resetar senha".
        operationName: "Resetar senha",

        // Optional. The type of the object affected by the operation. Like 'usuário'.
        // The default for resetPassword operation is "usuário", because only users have password.
        objectType: "usuário",

        // Optional. The name of the specific object affected by the operation. Like 'Jonas da Silva'.
        objectName: function () {
            throw "You must specify an objectName when deleting objects.";
        },

        // This field is going to be processed twice.
        // The first time it is formatted with 'options' object. Fields are like this '{field}'.
        // The second time it is formatted with 'data' object from json response. Fields are like this '{{field}}'
        successMessage: "Senha resetada para {{defaultPassword}}.",
    }
};
