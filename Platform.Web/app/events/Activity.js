/**
* @class App.events.Activity
* Обработчик клиентских событий сущности Activity
*/
Ext.define('App.events.Activity', {
    extend: 'App.events.CommonItem',

	events: [
	    { name: 'disable', handler: 'onDisableCode', item: 'Code' },
	    { name: 'change', handler: 'onChangeType', item: 'idActivityType' },
	    { name: 'blur', handler: 'onChangeCaption', item: 'Caption' },
	    { name: 'change', handler: 'onChangePPO', item: 'idPublicLegalFormation' }
	],

    isAutoCode: false,

    onDisableCode: function (sender) {
        if (isAutoCode && !sender.getValue()) {
            sender.setValue('Автоматически');
        }
    },

    onLoadPPO: function (result, response) {
        var obj = result.result[0];
        isAutoCode = (obj.idmethodofformingcode_activity == 1);
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

    onChangeCaption: function (sender, The, eOpts ) {
        if (this.getField('FullCaption').getValue() == '') {
            this.getField('FullCaption').setValue(sender.getValue());
        }
	},

	onChangeType: function (sender, newValue, oldValue) {
	    var b = newValue != 0 && newValue != 1;
	    if (b) {
	        this.getField('idPaidType').setValue(null, null);
	        this.getField('OrganSetPrice').setValue('');
	    }
	    this.getField('idPaidType').up().up().setVisible(!b);
	}
});