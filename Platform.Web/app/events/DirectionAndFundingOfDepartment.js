/**
* @class App.events.DirectionAndFundingOfDepartment
* Обработчик клиентских событий отчета "Направления и объемы финансирования деятельности ведомства"
*/
Ext.define('App.events.DirectionAndFundingOfDepartment', {
    extend: 'App.events.CommonItem',

    events: [
		{ name: 'change', handler: 'onChangeByApproved', item: 'byApproved' }
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
    }

});