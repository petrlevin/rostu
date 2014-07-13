/**
* @class App.events.User
* Обработчик клиентских событий сущности User
*/
Ext.define('App.events.User', {
	extend: 'App.events.CommonItem',

	events: [
		{ name: 'itemloaded', handler: 'onItemLoaded', item: null },
        { name: 'afterrender', handler: 'onAfterRender', item: null }
	],

	onItemLoaded: function (sender) {
	    this.getField('Name').disable();
	},

    onAfterRender: function (sender, newvalue, oldvalue) {
        this.getField('DateofLastEntry').disable();
    }
        
});