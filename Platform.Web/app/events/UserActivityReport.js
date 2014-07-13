/**
* @class App.events.UserActivityReport
* Обработчик клиентских событий сущности UserActivityReport
*/
Ext.define('App.events.UserActivityReport', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'change', handler: 'onChangeEntityLink', item: 'idEntity' },
        { name: 'afterrender', handler: 'onAfterRender', item: null }
    ],

    onAfterRender: function() {
        this.getField("idElement").disable();
    },

    onChangeEntityLink: function (sender, newValue, oldValue) {
        
        var entityId = this.getField("idEntity").getValue();

        var valueSelector = this.getField("idElement");

        valueSelector.clear();
        valueSelector.initialModel.identitylink = entityId;
        valueSelector.list = undefined;

        if (entityId) {
            valueSelector.enable();
        } else {
            valueSelector.disable();
        }

    }
});