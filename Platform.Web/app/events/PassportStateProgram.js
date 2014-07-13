/**
* @class App.events.VcpOmStatePrg
* Обработчик клиентских событий отчета "Перечень ведомственных целевых программ и основных мероприятий государственной программы"
*/
Ext.define('App.events.PassportStateProgram', {
    extend: 'App.events.CommonItem',

    events: [
		{ name: 'change', handler: 'onChangeByApproved', item: 'buildReportApprovedData' }
    ],

    onChangeByApproved: function (sender, newvalue, oldvalue) {
        var fdt = this.getField('DateReport');
        if (newvalue) {
            fdt.allowBlank = false;
            fdt.enable();
            fdt.setValue(new Date());
        }
        else {
            fdt.allowBlank = true;
            fdt.setValue(null);
            fdt.disable();
        }
    }

});
