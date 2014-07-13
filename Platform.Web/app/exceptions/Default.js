Ext.define("App.exceptions.Default", {

	requires: [
		'Ext.window.MessageBox'
	],

    handle: function (exception) {
        Ext.MessageBox.show({
            title: Locale.APP_MESSAGE_EXCEPTION,
            msg: "Выполнить операцию не удалось<br/><a target='_blank' href='" + exception.Url + "'>Информация для разработчиков</a>",
            buttons: Ext.Msg.OK,
            icon: Ext.Msg.WARNING
        });
    }
})