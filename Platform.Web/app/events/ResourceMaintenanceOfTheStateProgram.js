/**
* @class App.events.ResourceMaintenanceOfTheStateProgram
* Обработчик клиентских событий отчета "Ресурсное обеспечение реализации государственной программы"
*/
Ext.define('App.events.ResourceMaintenanceOfTheStateProgram', {
    extend: 'App.events.CommonItem',

    events: [
		{ name: 'change', handler: 'onChangeByApproved', item: 'byApproved' },
        { name: 'change', handler: 'onChangeshowActivities', item: 'showActivities' },
        { name: 'dataget', handler: 'onLoad', item: null }
    ],

    onChangeByApproved: function (sender, newvalue, oldvalue) {
        var fadd = this.getField('isRatingAdditionalNeeds');
        var fdt = this.getField('DateReport');
        if (newvalue) {
            fdt.enable();
            fdt.setValue(new Date());
            fdt.allowBlank = false;
            fadd.setValue(false);
            fadd.disable();
        }
        else {
            fdt.allowBlank = true;
            fdt.setValue(null);
            fdt.disable();
            fadd.enable();
        }
    },

    onChangeshowActivities: function (sender, newvalue, oldvalue) {
        var fnhf = this.getField('HasNoFunds');
        if (newvalue) {
            fnhf.enable();
        } else {
            fnhf.setValue(false);
            fnhf.disable();
        }
    },
    
    onLoad: function () {
        var flag = this.getField('showActivities').getValue();
        var fnhf = this.getField('HasNoFunds');
        if (flag) {
            fnhf.enable();
        } else {
            fnhf.setValue(false);
            fnhf.disable();
        }
    }
});