/**
* @class App.events.EditionsComparision
* Обработчик клиентских событий сущности "Сравнение редакций"
*/


Ext.define('App.events.EditionsComparision', {
	extend: 'App.events.CommonItem',

	events: [
	    { name: 'change', handler: 'onChangeAEntity', item: 'idEditionAEntity' },
	    { name: 'afterrender', handler: 'onBeforeShow', item: null }
	],

	onChangeAEntity: function (sender, newValue, oldValue) {
	    var entA = this.getField('idEditionAEntity').fieldValue;
	    var fieldBEntity = this.getField('idEditionBEntity');
	    
	    fieldBEntity.setValue(entA);
	},
	
    onBeforeShow: function() {
        var fieldBEntity = this.getField('idEditionBEntity');
        fieldBEntity.setReadOnly(true);
    }
});
