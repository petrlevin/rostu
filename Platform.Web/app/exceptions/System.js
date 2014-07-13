Ext.define("App.exceptions.System", {

	requires: [
		'Ext.window.MessageBox'
	],

    handle: function (exception) {
        Ext.MessageBox.show({
            title: 'Системное сообщение',
            msg: exception.message,
            buttons: Ext.Msg.OK,
            icon: Ext.Msg.WARNING
        });
    }
})