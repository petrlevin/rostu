Ext.define("App.exceptions.UserFriendly", {

	requires: [
		'Ext.window.MessageBox'
	],

    handle: function (exception) {
        Ext.MessageBox.show({
            title: "Системный контроль",
            msg: "Операция: " + exception.OperationDescription + "<br/>" + (exception.ExceptionTypeDescription ? "Причина: " + exception.ExceptionTypeDescription + "<br/>" : "") + exception.message,
            buttons: Ext.Msg.OK,
            icon: Ext.Msg.WARNING
        });
    }
})