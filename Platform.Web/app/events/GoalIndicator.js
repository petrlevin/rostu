/**
* @class App.events.GoalIndicator
* Обработчик клиентских событий сущности "Целевые показатели"
*/
Ext.define('App.events.GoalIndicator', {
	extend: 'App.events.CommonItem',

	events: [
	    { name: 'disable', handler: 'onDisableCode', item: 'Code' },
	    { name: 'change', handler: 'onChangePPO', item: 'idPublicLegalFormation' }
	],
	
	isAutoCode: false,
	
	onDisableCode: function(sender) {
	    if (isAutoCode && !sender.getValue()) {
	        sender.setValue('Автоматически');
	    }
	},
	
	onLoadPPO: function (result, response) {
	    var obj = result.result[0];
	    isAutoCode = (obj.idmethodofformingcode_targetindicator == 1);
	    if (isAutoCode) {  // Auto = 1
	        this.getField('Code').disable();
        }
	},

	onChangePPO: function (sender, newValue, oldValue) {
	    var idPpo = sender.getValue();
	    var code = this.getField('Code');
	    if (idPpo && !code.getValue()) {
	        DataService.getItem(sender.initialModel.identitylink, sender.getValue(), this.onLoadPPO, this);
	    }
	}
});