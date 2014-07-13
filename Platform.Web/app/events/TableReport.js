Ext.define('App.events.TableReport', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null }
    ],

    onAfterRender: function () {

        this.addNewButton(this.getForm().panel.getDockedItems('toolbar[dock="bottom"]')[0], this.getButton(), 0);
    },
    
    getButton: function () {
        var me = this;
        return {
            text: 'Выполнить',
            listeners: {
                scope: this,
                click: function () {
                    var params = {
                        reportId: me.getField("id").getValue()
                    };
                    App.ReportLauncher.launch(params, 'tablereport');
                }
            }
        };
    }
});