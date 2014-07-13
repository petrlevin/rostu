/**
* @class App.events.FormElements
* Простейший обработчик клиентских событий сущности FormElement
*/
Ext.define('App.events.FormElements', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'change', handler: 'onChangeEntityField', item: 'idEntityField' }
    ],

    onChangeEntityField: function (sender, newValue, oldValue) {
        if (sender.list) {
            if (!this.getField('Name').getValue()) {
                this.getField('Name').setValue(sender.list.grid.getSelectionModel().getLastSelected().get('caption'));
            }
        }
        
    }
});