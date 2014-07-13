/**
* @class App.events.Form
* Простейший обработчик клиентских событий сущности Form
*/
Ext.define('App.events.Form', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'change', handler: 'onChangeEntity', item: 'idEntity' }
    ],

    onChangeEntity: function (sender, newValue, oldValue) {
        if (sender.list) {
            if (!this.getField('Name').getValue()) {
                this.getField('Name').setValue(sender.list.grid.getSelectionModel().getLastSelected().get('name'));
            }
            if (!this.getField('Caption').getValue()) {
                this.getField('Caption').setValue(sender.list.grid.getSelectionModel().getLastSelected().get('caption') + '()');
            }
        }
    }
});