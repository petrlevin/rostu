/**
* @class App.events.Entity
* Простейший обработчик клиентских событий сущности Entity
*/
Ext.define('App.events.Entity', {
	extend: 'App.events.CommonItem',

	events: [
		{
			name: 'change',
			handler: 'onChange',
			item: 'name'
		}
	],

	onChange: function(sender, newValue, oldValue) {

		// Обработчик события изменения поля
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Отладочное событие сработало!');
		}
		// </debug>
	}	
});