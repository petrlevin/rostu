Ext.define("App.exceptions.Control", {

	requires: [
		'Ext.window.MessageBox'
	],

    handle: function (exception) {
        Ext.MessageBox.show({
            title: exception.Caption,
            msg: exception.message,
            buttons: Ext.Msg.OK,
            icon: Ext.Msg.WARNING
        });
    }
})