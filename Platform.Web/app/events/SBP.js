/**
* @class App.events.SBP
* Обработчик клиентских событий сущности "Субъекты бюджетного планирования"
*/
Ext.define('App.events.SBP', {
	extend: 'App.events.CommonItem',

	events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'enable', handler: 'enableIsFounder', item: 'IsFounder' },
	    { name: 'change', handler: 'onChangeFounder', item: 'IsFounder' },
	    { name: 'change', handler: 'onChangeParent', item: 'idParent' },
	    { name: 'change', handler: 'onChangeOrg',    item: 'idOrganization' },
	    { name: 'change', handler: 'onChangeType',   item: 'idSBPType' }
	],
	
	onAfterRender: function (sender) {
	    this.onChangeType(this.getField('idSBPType'));
	},
	
	enableIsFounder: function (sender) {
	    var typ = this.getField('idSBPType').getValue();
	    if (typ != 3) { // Не Казенное учереждение
	        sender.disable();
	    }
	},
	
	ctrlTabPanel: function(isHide1, isHide2) {
	    var f1 = this.getField('tpSBP_Blank');
	    var tp = f1.up('tabpanel');
	    var p1 = f1.up('panel');
	    var p2 = this.getField('tpSBP_PlanningPeriodsInDocumentsAUBU').up('panel');

	    if (isHide1 && isHide2) {
	        tp.hide();
	    } else {
	        if (isHide1) {
	            p1.tab.hide();
	        } else {
	            p1.tab.show();
	        }
	        if (isHide2) {
	            p2.tab.hide();
	        } else {
	            p2.tab.show();
	        }
	        tp.show();
        }
	},
	
	onChangeFounderOrType: function (sender) {
	    var typ = this.getField('idSBPType').getValue();
	    var founder = this.getField('IsFounder').getValue();

	    var isHide1;
	    var isHide2;
	    
	    if (typ == 3 && founder) { // Казенное учереждение и флаг учередитель
	        isHide2 = false;
	    } else {
	        isHide2 = true;
	    }

	    if ( typ == null
	        || typ == 4 || typ == 5      // Бюджетное учреждение или Автономное учреждение
	        || (typ == 3 && !founder) // или (Казенное учреждение и Учредитель = Ложь)
	    ) { 
	        isHide1 = true;
	    } else {
	        isHide1 = false;
	    }

	    this.ctrlTabPanel(isHide1, isHide2);
	},
	
	onChangeFounder: function (sender) {
	    this.onChangeFounderOrType(sender);
	},

	onChangeParent: function (sender, newValue, oldValue) {
	    var idP = sender.getValue();
	    var typ = this.getField('idSBPType').getValue();
	    if (idP && sender.list && typ != 4 && typ != 5) { // не автономное и не бюджетное
	        DataService.getItem(sender.initialModel.identitylink, idP, this.onLoadParent, this);
	    }
	},

	onLoadParent: function (result, response) {
	    var obj = result.result[0];
	    this.getField('idKVSR').setValue(obj.idkvsr, obj.idkvsr_caption);
	},
	
    setCap: function (cap, typ) {
        var v = cap;
        if (typ == 1) { // Главный распорядитель БС
            v = v + ' (ГРБС)';
        } else if (typ == 2) { // Распорядитель БС
            v = v + ' (РБС)';
        } else if (typ == 3) { // Казенное учреждение 
            v = v + ' (КУ)';
        } else if (typ == 4) { // Бюджетное учреждение
            v = v + ' (БУ)';
        } else if (typ == 5) { // Автономное учреждение
            v = v + ' (АУ)';
        }
        this.getField('Caption').setValue(v);
    },

	onChangeType: function (sender, newValue, oldValue) {
        var typ = sender.getValue();
        var f = this.getField('IsFounder');

        var org = this.getField('idOrganization');
	    if (org) {
	        var idOrg = org.getValue();
	        if (idOrg) {
	            DataService.getItem(org.initialModel.identitylink, idOrg, this.onLoadOrg, this);
	        }
	    }
	    
	    if (typ != 3) { // Не Казенное учереждение
	        f.setValue(false);
            f.disable();
            this.setFieldVisibility('IsFounder', false);
        } else {
            f.enable();
            this.setFieldVisibility('IsFounder', true);
        }
        this.onChangeFounderOrType(sender);
	},
	
	onChangeOrg: function(sender, newValue, oldValue) {
	    var org = sender.getValue();
	    if (sender.list && org) {
	        DataService.getItem(sender.initialModel.identitylink, org, this.onLoadOrg, this);
	    }
    },
	
	onLoadOrg: function (result, response) {
	    this.setCap(result.result[0].caption, this.getField('idSBPType').getValue());
	}
});