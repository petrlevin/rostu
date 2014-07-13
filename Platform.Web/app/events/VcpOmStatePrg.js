/**
* @class App.events.VcpOmStatePrg
* Обработчик клиентских событий отчета "Перечень ведомственных целевых программ и основных мероприятий государственной программы"
*/
Ext.define('App.events.VcpOmStatePrg', {
    extend: 'App.events.CommonItem',

    events: [
		{ name: 'change', handler: 'onChangeByApproved', item: 'ConstructReportApprovedData' }
    ],

    onChangeByApproved: function (sender, newvalue, oldvalue) {
        var fdt = this.getField('DateReport');
        if (newvalue) {
            fdt.enable();
            fdt.setValue(new Date());
        }
        else {
            fdt.setValue(null);
            fdt.disable();
        }
    }

});
