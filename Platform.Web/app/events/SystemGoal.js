/**
* @class App.events.SystemGoal
* Обработчик клиентских событий сущности "Система целеполагания"
*/
Ext.define('App.events.SystemGoal', {
	extend: 'App.events.CommonItem',

	events: [
		{ name: 'disable', handler: 'onDisableCode', item: 'Code' },
	    { name: 'change',  handler: 'onElementTypeSystemGoal', item: 'idElementTypeSystemGoal' },
	    { name: 'change',  handler: 'onChangeTypeCommitDoc',   item: 'idDocType_CommitDoc' },
	    { name: 'change',  handler: 'onChangePPO',             item: 'idPublicLegalFormation' }
	],
	
	isAutoCode: false,
	
	onDisableCode: function(sender) {
	    if (isAutoCode && !sender.getValue()) {
	        sender.setValue('Автоматически');
	    }
	},
	
	onLoadDefaultTypeDoc: function (result, response) {
	    var f = this.getField('IdDocType_CommitDoc');
	    if (result.id && !f.getValue()) {
	        f.setValue(result.id, result.caption);
	    }
	},
	
	onElementTypeSystemGoal: function (sender, newValue, oldValue) {
	    if (newValue) {
	        SborCommonService.getElementTypeSystemGoalDefaultTypeDoc(newValue, this.onLoadDefaultTypeDoc, this);
	    }
	},
	
	onLoadPPO: function (result, response) {
	    var obj = result.result[0];
	    isAutoCode = (obj.idmethodofformingcode_goalsetting == 1);
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
	},
	
	onLoadDocType: function (result, response) {
	    var f = this.getField('idSBP');
	    var obj = result.result[0];
	    if (obj.identity == -1543503797) { // документ Деятельность ведомства
	        f.allowBlank = false;
	    } else {
	        f.allowBlank = true;
	    }
	    f.clearInvalid();
	    f.isValid();
	},

	onChangeTypeCommitDoc: function (sender, newValue, oldValue) {
	    var doctyp = sender.getValue();
	    if (doctyp) {
	        DataService.getItem(sender.initialModel.identitylink, doctyp, this.onLoadDocType, this);
	    }
	}
});