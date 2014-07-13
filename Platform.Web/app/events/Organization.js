/**
* @class App.events.Organization
* Обработчик клиентских событий сущности "Организации"
*/
Ext.define('App.events.Organization', {
    extend: 'App.events.CommonItem',

    events: [
	    { name: 'change', handler: 'onChangeCaption', item: 'Caption' }
    ],

    onChangeCaption: function (sender, newValue, oldValue) {
        if (oldValue != undefined && newValue) {
            //this.getField('Description').setValue(newValue);
        }
    }
	
});