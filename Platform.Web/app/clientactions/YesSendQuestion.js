Ext.define("App.clientactions.YesSendQuestion", {

    requires: [
        'Ext.window.MessageBox'
    ],
 
    constructor: function(config) {

        Ext.apply(this, config);
    },

    execute: function(callback, scope) {

        Ext.Msg.show({
            title: this.Title || 'Вопрос от сервера',
            buttons: Ext.MessageBox.YESNO,
            icon: Ext.Msg.QUESTION,
            msg: this.Message,
            fn: function(btn) {

                if (btn == 'yes') {
                    Ext.callback(callback, scope, [{}]);
                } else {
                    Ext.callback(callback, scope, [{}, true]);
                }
            }
        });
    }
})